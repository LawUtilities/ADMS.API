using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ADMS.API.Services
{
    /// <summary>
    ///   Interface for model validation
    /// </summary>
    public interface IModelValidator
    {
        /// <summary>
        ///       Validates the model and returns a boolean indicating success or failure.
        /// </summary>
        /// <param name="model">Model to be validated.</param>
        /// <param name="modelState">Model state to be returned.</param>
        /// <returns>Model state</returns>
        bool TryValidate(object model, out ModelStateDictionary modelState);
    }
}
