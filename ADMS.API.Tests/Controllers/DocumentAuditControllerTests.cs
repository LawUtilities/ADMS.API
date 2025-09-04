using ADMS.API.Controllers;
using ADMS.API.Entities;
using ADMS.API.Models;
using ADMS.API.ResourceParameters;
using ADMS.API.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;

using Moq;

namespace ADMS.API.Tests.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="DocumentAuditController"/>.
    /// </summary>
    public class DocumentAuditControllerTests
    {
        private readonly Mock<ILogger<DocumentAuditController>> _loggerMock = new();
        private readonly Mock<IAdmsRepository> _repoMock = new();
        private readonly Mock<IPropertyMappingService> _propertyMappingServiceMock = new();
        private readonly Mock<ProblemDetailsFactory> _problemDetailsFactoryMock = new();
        private readonly Mock<IValidationService> _validationServiceMock = new();

        private DocumentAuditController CreateController()
        {
            var controller = new DocumentAuditController(
                _loggerMock.Object,
                _repoMock.Object,
                _propertyMappingServiceMock.Object,
                _problemDetailsFactoryMock.Object,
                _validationServiceMock.Object
            )
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
            return controller;
        }

        #region GetDocumentActivityAuditsAsync

        /// <summary>
        /// Tests that a 400 BadRequest is returned when the model state is invalid.
        /// </summary>
        [Fact]
        public async Task GetDocumentActivityAuditsAsync_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            var controller = CreateController();
            controller.ModelState.AddModelError("Test", "Invalid");

            var result = await controller.GetDocumentActivityAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), new DocumentAuditsResourceParameters());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        }

        /// <summary>
        /// Tests that a 404 NotFound is returned when the matter does not exist.
        /// </summary>
        [Fact]
        public async Task GetDocumentActivityAuditsAsync_ReturnsNotFound_WhenMatterDoesNotExist()
        {
            var controller = CreateController();
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.GetDocumentActivityAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), new DocumentAuditsResourceParameters());

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            var details = Assert.IsType<ProblemDetails>(notFound.Value);
            Assert.Equal("Matter not found.", details.Title);
        }

        /// <summary>
        /// Tests that a 404 NotFound is returned when the document does not exist.
        /// </summary>
        [Fact]
        public async Task GetDocumentActivityAuditsAsync_ReturnsNotFound_WhenDocumentDoesNotExist()
        {
            var controller = CreateController();
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.GetDocumentActivityAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), new DocumentAuditsResourceParameters());

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            var details = Assert.IsType<ProblemDetails>(notFound.Value);
            Assert.Equal("Document not found.", details.Title);
        }

        /// <summary>
        /// Tests that a 400 BadRequest is returned when the order by field is invalid.
        /// </summary>
        [Fact]
        public async Task GetDocumentActivityAuditsAsync_ReturnsBadRequest_WhenOrderByIsInvalid()
        {
            var controller = CreateController();
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _propertyMappingServiceMock
                .Setup(p => p.ValidMappingExistsFor<DocumentActivityUserMinimalDto, DocumentActivityUserMinimalDto>(It.IsAny<string>()))
                .Returns(false);

            var resourceParams = new DocumentAuditsResourceParameters { OrderBy = "InvalidField" };

            var result = await controller.GetDocumentActivityAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), resourceParams);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        }

        /// <summary>
        /// Tests that a 404 NotFound is returned when no audit records are found.
        /// </summary>
        [Fact]
        public async Task GetDocumentActivityAuditsAsync_ReturnsNotFound_WhenNoAuditsFound()
        {
            var controller = CreateController();
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _propertyMappingServiceMock
                .Setup(p => p.ValidMappingExistsFor<DocumentActivityUserMinimalDto, DocumentActivityUserMinimalDto>(It.IsAny<string>()))
                .Returns(true);
            _repoMock
                .Setup(r => r.GetDocumentActivityAuditsAsync(It.IsAny<Guid>(), It.IsAny<DocumentAuditsResourceParameters>()))!
                .ReturnsAsync((ActionResult<Helpers.PagedList<DocumentActivityUserMinimalDto>>?)null);

            var result = await controller.GetDocumentActivityAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), new DocumentAuditsResourceParameters());

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            var details = Assert.IsType<ProblemDetails>(notFound.Value);
            Assert.Equal("No audit records found.", details.Title);
        }

        /// <summary>
        /// Tests that a 200 OK is returned with audit records when successful.
        /// </summary>
        [Fact]
        public async Task GetDocumentActivityAuditsAsync_ReturnsOk_WithAuditRecords()
        {
            var controller = CreateController();
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _propertyMappingServiceMock
                .Setup(p => p.ValidMappingExistsFor<DocumentActivityUserMinimalDto, DocumentActivityUserMinimalDto>(It.IsAny<string>()))
                .Returns(true);

            var pagedList = new Helpers.PagedList<DocumentActivityUserMinimalDto>(
                [
                    new()
                    {
                        DocumentId = Guid.NewGuid(),
                        DocumentActivity = new DocumentActivityMinimalDto { Activity = "CREATED" },
                        User = new UserMinimalDto()
                        {
                            Id = Guid.NewGuid(),
                            Name = "John Dow"
                        },
                        CreatedAt = DateTime.UtcNow
                    }
                ],
                1, 1, 1
            );
            var actionResult = new ActionResult<Helpers.PagedList<DocumentActivityUserMinimalDto>>(pagedList);

            _repoMock
                .Setup(r => r.GetDocumentActivityAuditsAsync(It.IsAny<Guid>(), It.IsAny<DocumentAuditsResourceParameters>()))
                .ReturnsAsync(actionResult);

            var result = await controller.GetDocumentActivityAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), new DocumentAuditsResourceParameters());

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<Helpers.PagedList<DocumentActivityUserMinimalDto>>(okResult.Value);
            Assert.Single(returned);
        }

        /// <summary>
        /// Tests that a 500 InternalServerError is returned when an exception is thrown.
        /// </summary>
        [Fact]
        public async Task GetDocumentActivityAuditsAsync_ReturnsInternalServerError_OnException()
        {
            var controller = CreateController();
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _propertyMappingServiceMock
                .Setup(p => p.ValidMappingExistsFor<DocumentActivityUserMinimalDto, DocumentActivityUserMinimalDto>(It.IsAny<string>()))
                .Returns(true);

            _repoMock
                .Setup(r => r.GetDocumentActivityAuditsAsync(It.IsAny<Guid>(), It.IsAny<DocumentAuditsResourceParameters>()))
                .ThrowsAsync(new Exception("Test exception"));

            _problemDetailsFactoryMock
                .Setup(f => f.CreateProblemDetails(
                    It.IsAny<HttpContext>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred. Please try again later."
                });

            var result = await controller.GetDocumentActivityAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), new DocumentAuditsResourceParameters());

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            var details = Assert.IsType<ProblemDetails>(objectResult.Value);
            Assert.Equal("An unexpected error occurred. Please try again later.", details.Title);
        }

        #endregion

        #region GetDocumentMoveFromAuditsAsync

        /// <summary>
        /// Tests that a 400 BadRequest is returned when the model state is invalid for move-from audits.
        /// </summary>
        [Fact]
        public async Task GetDocumentMoveFromAuditsAsync_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            var controller = CreateController();
            controller.ModelState.AddModelError("Test", "Invalid");

            var result = await controller.GetDocumentMoveFromAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), new DocumentAuditsResourceParameters());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        }

        /// <summary>
        /// Tests that a 404 NotFound is returned when the matter does not exist for move-from audits.
        /// </summary>
        [Fact]
        public async Task GetDocumentMoveFromAuditsAsync_ReturnsNotFound_WhenMatterDoesNotExist()
        {
            var controller = CreateController();
            var notFoundResult = new NotFoundResult();
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(notFoundResult);

            var result = await controller.GetDocumentMoveFromAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), new DocumentAuditsResourceParameters());

            Assert.Equal(notFoundResult, result.Result);
        }

        /// <summary>
        /// Tests that a 404 NotFound is returned when the document does not exist for move-from audits.
        /// </summary>
        [Fact]
        public async Task GetDocumentMoveFromAuditsAsync_ReturnsNotFound_WhenDocumentDoesNotExist()
        {
            var controller = CreateController();
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            var notFoundResult = new NotFoundResult();
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(notFoundResult);

            var result = await controller.GetDocumentMoveFromAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), new DocumentAuditsResourceParameters());

            Assert.Equal(notFoundResult, result.Result);
        }

        /// <summary>
        /// Tests that a 400 BadRequest is returned when the order by field is invalid for move-from audits.
        /// </summary>
        [Fact]
        public async Task GetDocumentMoveFromAuditsAsync_ReturnsBadRequest_WhenOrderByIsInvalid()
        {
            var controller = CreateController();
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _propertyMappingServiceMock
                .Setup(p => p.ValidMappingExistsFor<MatterDocumentActivityUserMinimalDto, MatterDocumentActivityUserMinimalDto>(It.IsAny<string>()))
                .Returns(false);

            var resourceParams = new DocumentAuditsResourceParameters { OrderBy = "InvalidField" };

            var result = await controller.GetDocumentMoveFromAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), resourceParams);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        }

        /// <summary>
        /// Tests that a 404 NotFound is returned when no move-from audit records are found.
        /// </summary>
        [Fact]
        public async Task GetDocumentMoveFromAuditsAsync_ReturnsNotFound_WhenNoAuditsFound()
        {
            var controller = CreateController();
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _propertyMappingServiceMock
                .Setup(p => p.ValidMappingExistsFor<MatterDocumentActivityUserMinimalDto, MatterDocumentActivityUserMinimalDto>(It.IsAny<string>()))
                .Returns(true);
            _repoMock
                .Setup(r => r.GetPaginatedDocumentMoveFromAuditsAsync(It.IsAny<Guid>(), It.IsAny<DocumentAuditsResourceParameters>()))!
                .ReturnsAsync((Helpers.PagedList<MatterDocumentActivityUserMinimalDto>?)null);

            var result = await controller.GetDocumentMoveFromAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), new DocumentAuditsResourceParameters());

            Assert.IsType<NotFoundResult>(result.Result);
        }

        /// <summary>
        /// Tests that a 200 OK is returned with move-from audit records when successful.
        /// </summary>
        [Fact]
        public async Task GetDocumentMoveFromAuditsAsync_ReturnsOk_WithAuditRecords()
        {
            var controller = CreateController();
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _propertyMappingServiceMock
                .Setup(p => p.ValidMappingExistsFor<MatterDocumentActivityUserMinimalDto, MatterDocumentActivityUserMinimalDto>(It.IsAny<string>()))
                .Returns(true);

            var matter = new Matter()
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };

            var user = new UserMinimalDto
            {
                Id = Guid.NewGuid(),
                Name = "Jane Doe"
            };

            var matterDocumentActivity = new MatterDocumentActivityMinimalDto
            {
                Id = Guid.NewGuid(),
                Activity = "MOVED"
            };

            var pagedList = new Helpers.PagedList<MatterDocumentActivityUserMinimalDto>(
                [
                    new MatterDocumentActivityUserMinimalDto
                    {
                        DocumentId = Guid.NewGuid(),
                        MatterId = matter.Id,
                        UserId = user.Id,
                        User = user,
                        CreatedAt = DateTime.UtcNow,
                        MatterDocumentActivityId = Guid.NewGuid(),
                        MatterDocumentActivity = matterDocumentActivity
                    }
                ],
                1, 1, 1
            );

            /*
            var actionResult = new ActionResult<Helpers.PagedList<MatterDocumentActivityUserMinimalDto>>(pagedList);
            */

            _repoMock
                .Setup(r => r.GetPaginatedDocumentMoveFromAuditsAsync(It.IsAny<Guid>(), It.IsAny<DocumentAuditsResourceParameters>()))
                .ReturnsAsync(pagedList);

            var result = await controller.GetDocumentMoveFromAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), new DocumentAuditsResourceParameters());

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<Helpers.PagedList<MatterDocumentActivityUserMinimalDto>>(okResult.Value);
            Assert.Single(returned);
        }

        /// <summary>
        /// Tests that a 500 InternalServerError is returned when an exception is thrown for move-from audits.
        /// </summary>
        [Fact]
        public async Task GetDocumentMoveFromAuditsAsync_ReturnsInternalServerError_OnException()
        {
            var controller = CreateController();
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _propertyMappingServiceMock
                .Setup(p => p.ValidMappingExistsFor<MatterDocumentActivityUserMinimalDto, MatterDocumentActivityUserMinimalDto>(It.IsAny<string>()))
                .Returns(true);

            _repoMock
                .Setup(r => r.GetPaginatedDocumentMoveFromAuditsAsync(It.IsAny<Guid>(), It.IsAny<DocumentAuditsResourceParameters>()))
                .ThrowsAsync(new Exception("Test exception"));

            _problemDetailsFactoryMock
                .Setup(f => f.CreateProblemDetails(
                    It.IsAny<HttpContext>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred. Please try again later."
                });

            var result = await controller.GetDocumentMoveFromAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), new DocumentAuditsResourceParameters());

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            var details = Assert.IsType<ProblemDetails>(objectResult.Value);
            Assert.Equal("An unexpected error occurred. Please try again later.", details.Title);
        }

        #endregion

        #region GetDocumentMoveToAuditsAsync

        /// <summary>
        /// Tests that a 400 BadRequest is returned when the model state is invalid for move-to audits.
        /// </summary>
        [Fact]
        public async Task GetDocumentMoveToAuditsAsync_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            var controller = CreateController();
            controller.ModelState.AddModelError("Test", "Invalid");

            var result = await controller.GetDocumentMoveToAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), new DocumentAuditsResourceParameters());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        }

        /// <summary>
        /// Tests that a 404 NotFound is returned when the matter does not exist for move-to audits.
        /// </summary>
        [Fact]
        public async Task GetDocumentMoveToAuditsAsync_ReturnsNotFound_WhenMatterDoesNotExist()
        {
            var controller = CreateController();
            var notFoundResult = new NotFoundResult();
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(notFoundResult);

            var result = await controller.GetDocumentMoveToAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), new DocumentAuditsResourceParameters());

            Assert.Equal(notFoundResult, result.Result);
        }

        /// <summary>
        /// Tests that a 404 NotFound is returned when the document does not exist for move-to audits.
        /// </summary>
        [Fact]
        public async Task GetDocumentMoveToAuditsAsync_ReturnsNotFound_WhenDocumentDoesNotExist()
        {
            var controller = CreateController();
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            var notFoundResult = new NotFoundResult();
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(notFoundResult);

            var result = await controller.GetDocumentMoveToAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), new DocumentAuditsResourceParameters());

            Assert.Equal(notFoundResult, result.Result);
        }

        /// <summary>
        /// Tests that a 400 BadRequest is returned when the order by field is invalid for move-to audits.
        /// </summary>
        [Fact]
        public async Task GetDocumentMoveToAuditsAsync_ReturnsBadRequest_WhenOrderByIsInvalid()
        {
            var controller = CreateController();
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _propertyMappingServiceMock
                .Setup(p => p.ValidMappingExistsFor<MatterDocumentActivityUserMinimalDto, MatterDocumentActivityUserMinimalDto>(It.IsAny<string>()))
                .Returns(false);

            var resourceParams = new DocumentAuditsResourceParameters { OrderBy = "InvalidField" };

            var result = await controller.GetDocumentMoveToAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), resourceParams);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        }

        /// <summary>
        /// Tests that a 404 NotFound is returned when no move-to audit records are found.
        /// </summary>
        [Fact]
        public async Task GetDocumentMoveToAuditsAsync_ReturnsNotFound_WhenNoAuditsFound()
        {
            var controller = CreateController();
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _propertyMappingServiceMock
                .Setup(p => p.ValidMappingExistsFor<MatterDocumentActivityUserMinimalDto, MatterDocumentActivityUserMinimalDto>(It.IsAny<string>()))
                .Returns(true);
                _repoMock
                    .Setup(r => r.GetPaginatedDocumentMoveFromAuditsAsync(It.IsAny<Guid>(), It.IsAny<DocumentAuditsResourceParameters>()))!
                .ReturnsAsync((Helpers.PagedList<MatterDocumentActivityUserMinimalDto>?)null);

            var result = await controller.GetDocumentMoveToAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), new DocumentAuditsResourceParameters());

            Assert.IsType<NotFoundResult>(result.Result);
        }

        /// <summary>
        /// Tests that a 200 OK is returned with move-to audit records when successful.
        /// </summary>
        [Fact]
        public async Task GetDocumentMoveToAuditsAsync_ReturnsOk_WithAuditRecords()
        {
            var controller = CreateController();
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _propertyMappingServiceMock
                .Setup(p => p.ValidMappingExistsFor<MatterDocumentActivityUserMinimalDto, MatterDocumentActivityUserMinimalDto>(It.IsAny<string>()))
                .Returns(true);

            var matter = new Matter()
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };

            var user = new UserMinimalDto
            {
                Id = Guid.NewGuid(),
                Name = "Jane Doe"
            };

            var matterDocumentActivity = new MatterDocumentActivityMinimalDto
            {
                Id = Guid.NewGuid(),
                Activity = "MOVED"
            };

            var pagedList = new Helpers.PagedList<MatterDocumentActivityUserMinimalDto>(
                [
                    new MatterDocumentActivityUserMinimalDto
                    {
                        DocumentId = Guid.NewGuid(),
                        MatterId = matter.Id,
                        UserId = user.Id,
                        User = user,
                        MatterDocumentActivityId = matterDocumentActivity.Id,
                        MatterDocumentActivity = matterDocumentActivity,
                        CreatedAt = DateTime.UtcNow
                    }
                ],
                1, 1, 1
            );
            var actionResult = pagedList;

            _repoMock
                .Setup(r => r.GetPaginatedDocumentMoveFromAuditsAsync(It.IsAny<Guid>(), It.IsAny<DocumentAuditsResourceParameters>()))
                .ReturnsAsync(actionResult);

            var result = await controller.GetDocumentMoveToAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), new DocumentAuditsResourceParameters());

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<Helpers.PagedList<MatterDocumentActivityUserMinimalDto>>(okResult.Value);
            Assert.Single(returned);
        }

        /// <summary>
        /// Tests that a 500 InternalServerError is returned when an exception is thrown for move-to audits.
        /// </summary>
        [Fact]
        public async Task GetDocumentMoveToAuditsAsync_ReturnsInternalServerError_OnException()
        {
            var controller = CreateController();
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _propertyMappingServiceMock
                .Setup(p => p.ValidMappingExistsFor<MatterDocumentActivityUserMinimalDto, MatterDocumentActivityUserMinimalDto>(It.IsAny<string>()))
                .Returns(true);

            _repoMock
                .Setup(r => r.GetPaginatedDocumentMoveToAuditsAsync(It.IsAny<Guid>(), It.IsAny<DocumentAuditsResourceParameters>()))
                .ThrowsAsync(new Exception("Test exception"));

            _problemDetailsFactoryMock
                .Setup(f => f.CreateProblemDetails(
                    It.IsAny<HttpContext>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred. Please try again later."
                });

            var result = await controller.GetDocumentMoveToAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), new DocumentAuditsResourceParameters());

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            var details = Assert.IsType<ProblemDetails>(objectResult.Value);
            Assert.Equal("An unexpected error occurred. Please try again later.", details.Title);
        }

        #endregion
    }
}