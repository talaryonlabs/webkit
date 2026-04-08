import jetbrains.buildServer.configs.kotlin.*
import jetbrains.buildServer.configs.kotlin.buildFeatures.commitStatusPublisher
import jetbrains.buildServer.configs.kotlin.buildFeatures.perfmon
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetBuild
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetNugetPush
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetPack
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetRestore
import jetbrains.buildServer.configs.kotlin.buildSteps.script
import jetbrains.buildServer.configs.kotlin.triggers.vcs
import jetbrains.buildServer.configs.kotlin.vcs.GitVcsRoot

/*
The settings script is an entry point for defining a TeamCity
project hierarchy. The script should contain a single call to the
project() function with a Project instance or an init function as
an argument.

VcsRoots, BuildTypes, Templates, and subprojects can be
registered inside the project using the vcsRoot(), buildType(),
template(), and subProject() methods respectively.

To debug settings scripts in command-line, run the

    mvnDebug org.jetbrains.teamcity:teamcity-configs-maven-plugin:generate

command and attach your debugger to the port 8000.

To debug in IntelliJ Idea, open the 'Maven Projects' tool window (View
-> Tool Windows -> Maven Projects), find the generate task node
(Plugins -> teamcity-configs -> teamcity-configs:generate), the
'Debug' option is available in the context menu for the task.
*/

version = "2025.11"

project {

    vcsRoot(HttpsGithubComTalaryonlabsToolboxRefsHeadsDev)

    buildType(Webkit)
}

object Webkit : BuildType({
    name = "WebKit"

    params {
        param("env.version", "-")
        param("env.package", "-")
        param("env.state", "-")
        param("local.projectName", "Toolbox")
    }

    steps {
        script {
            name = "Get Version Number"
            id = "Get_Version_Number"
            workingDir = "src/%local.projectName%"
            scriptContent = """
                #!/bin/bash
                SOURCE="%nuget.source.talaryon%"
                VERSION="${'$'}(cat %local.projectName%.csproj | grep -Eo '<Version>[0-9.\-]+</Version>' | grep -Eo '[0-9.\-]+')"
                PACKAGE_NAME="${'$'}(grep -Eo '<PackageId>[A-Za-z0-9.\-]+</PackageId>' %local.projectName%.csproj | sed -E 's/<PackageId>([A-Za-z0-9.\-]+)<\/PackageId>/\1/')"
                API_URL="https://${'$'}SOURCE/v3/registration/${'$'}PACKAGE_NAME/index.json"
                RESPONSE=${'$'}(curl -s "${'$'}API_URL" | jq -r '.items[].items[].catalogEntry.version')
                
                if echo "${'$'}RESPONSE" | grep -q "${'$'}VERSION"; then
                	echo "##teamcity[setParameter name='env.state' value='exists']"
                    echo "Version ${'$'}VERSION already exists on ${'$'}SOURCE."
                fi
                
                echo "##teamcity[setParameter name='env.package' value='${'$'}PACKAGE_NAME']"
                echo "##teamcity[setParameter name='env.version' value='${'$'}VERSION']"
                echo "##teamcity[buildNumber '${'$'}PACKAGE_NAME:${'$'}VERSION']"
            """.trimIndent()
        }
        dotnetRestore {
            name = "Restore Packages"
            id = "Restore_Packages"
            projects = "src/%local.projectName%/%local.projectName%.csproj"
            sources = "https://%nuget.source.talaryon%/v3/index.json"
        }
        dotnetBuild {
            name = "Build"
            id = "dotnet"
            projects = "src/%local.projectName%/%local.projectName%.csproj"
        }
        dotnetPack {
            name = "Pack NuGet Package"
            id = "Pack_NuGet_Package"

            conditions {
                doesNotEqual("env.state", "exists")
            }
            projects = "src/%local.projectName%/%local.projectName%.csproj"
            outputDir = "publish"
        }
        dotnetNugetPush {
            name = "Push NuGet Package"
            id = "Push_NuGet_Package"

            conditions {
                doesNotEqual("env.state", "exists")
            }
            packages = "publish/%env.package%.%env.version%.nupkg"
            serverUrl = "https://%nuget.source.talaryon%/v3/index.json"
            apiKey = "credentialsJSON:56baad1f-80c9-4e5e-8ad3-d684ac95dfb8"
        }
    }

    triggers {
        vcs {
        }
    }

    features {
        perfmon {
        }
        commitStatusPublisher {
            enabled = false
            vcsRootExtId = "Libraries_Toolbox_HttpsGithubComTalaryonlabsStackmgrRefsHeadsMain"
            publisher = github {
                githubUrl = "https://api.github.com"
                authType = personalToken {
                    token = "credentialsJSON:f395c44f-e583-4547-91ab-c3d8e4d49d97"
                }
            }
        }
    }

    requirements {
        contains("teamcity.agent.name", "build-ferociousbyte-dev")
    }
})

object HttpsGithubComTalaryonlabsToolboxRefsHeadsDev : GitVcsRoot({
    name = "https://github.com/talaryonlabs/toolbox#refs/heads/dev"
    url = "https://github.com/talaryonlabs/toolbox"
    branch = "refs/heads/dev"
    branchSpec = "refs/heads/*"
    authMethod = password {
        userName = "ferociousbyte"
        password = "credentialsJSON:6a79183f-823b-402a-95f9-6dfe2623f133"
    }
})
