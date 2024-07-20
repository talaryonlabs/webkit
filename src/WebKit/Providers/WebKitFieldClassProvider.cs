using Microsoft.AspNetCore.Components.Forms;

namespace Talaryon.WebKit.Providers;

public class WebKitFieldClassProvider : FieldCssClassProvider
{
    public override string GetFieldCssClass(EditContext editContext, in FieldIdentifier fieldIdentifier)
    {
        if (!editContext.IsModified())
        {
            return "";
        }
        return !editContext.GetValidationMessages(fieldIdentifier).Any() ? "is-success" : "is-error";
    }
}