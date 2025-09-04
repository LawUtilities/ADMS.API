using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Services
{
    /// <summary>
    ///   Interface for model validation
    /// </summary>
    public class ModelValidator : IModelValidator
    {
        private readonly IModelMetadataProvider _metadataProvider;

        /// <summary>
        ///      Initializes a new instance of the <see cref="ModelValidator"/> class.
        /// </summary>
        /// <param name="metadataProvider">Model metadata provider.</param>
        public ModelValidator(IModelMetadataProvider metadataProvider)
        {
            _metadataProvider = metadataProvider;
        }

        /// <summary>
        ///      Validates the model and returns a boolean indicating success or failure.
        /// </summary>
        /// <param name="model">Model to be validated.</param>
        /// <param name="modelState">Model state to return.</param>
        /// <returns>Model state</returns>
        public bool TryValidate(object model, out ModelStateDictionary modelState)
        {
            var validationContext = new ValidationContext(model);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model, validationContext, results, true);

            modelState = new ModelStateDictionary();
            foreach (var result in results)
            {
                modelState.AddModelError(string.Empty, result.ErrorMessage ?? String.Empty);
            }

            return isValid;
        }
    }
}
