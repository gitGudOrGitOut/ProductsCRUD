using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ProductsCRUD.AutomatedCacher.Model;
using ProductsCRUD.Controllers;
using ProductsCRUD.DomainModels;
using ProductsCRUD.DTOs;
using ProductsCRUD.Profiles;
using ProductsCRUD.Repositories.Interface;
using ProductsCRUDTests.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ProductsCRUDTests
{ 
    public class ProductControllerTest
    {
        private readonly IMapper _mapper;
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly IOptions<MemoryCacheModel> _memoryCacheModel;

        private ProductDomainModel[] GetTestProducts() => new ProductDomainModel[]
        {
            new ProductDomainModel {ProductID = 0, ProductName = "TestProduct", ProductDescription = "Tasty and savory.", ProductPrice = 1.56, ProductQuantity = 4},
            new ProductDomainModel {ProductID = 1, ProductName = "Heineken 0.5L", ProductDescription = "Blonde beer. Yummy.", ProductPrice = 2.56, ProductQuantity = 45},
            new ProductDomainModel { ProductID = 2, ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 },
            new ProductDomainModel { ProductID = 3, ProductName = "Nails", ProductDescription = "Pack of 50 nails.", ProductPrice = 4.5, ProductQuantity = 23 },
            new ProductDomainModel { ProductID = 4, ProductName = "Candle", ProductDescription = "Aroma like you've never smelled before.", ProductPrice = 3.56, ProductQuantity = 43 }
        };

        public ProductControllerTest()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ProductProfile());
            });

            _memoryCacheMock = new Mock<IMemoryCache>();
            object expectedValue = null;
            _memoryCacheMock
                .Setup(x => x.TryGetValue(It.IsAny<object>(), out expectedValue))
                .Returns(false);
            var options = Options.Create(new MemoryCacheModel());
            _memoryCacheModel = options;
            _mapper = config.CreateMapper();

        }

        [Fact]
        public async Task GetAll_WhenCalled_ReturnsAllItems()
        {
            // Arrange

            var expected = GetTestProducts();
            var mockProducts = new Mock<IProductsRepository>(MockBehavior.Strict);
            mockProducts.Setup(r => r.GetAllProductsAsync())
                .ReturnsAsync(expected)
                .Verifiable();

            var controller = new ProductController(mockProducts.Object, _mapper, _memoryCacheMock.Object, _memoryCacheModel);

            // Act
            var result = await controller.GetAllProducts();

            // Assert
            var viewResult = Assert.IsType<OkObjectResult>(result.Result);
            var model = Assert.IsAssignableFrom<IEnumerable<ProductReadDTO>>(
                viewResult.Value);
            Assert.Equal(expected.Length, model.Count());
            mockProducts.Verify(r => r.GetAllProductsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAll_WhenCalled_ReturnsCorrectItems()
        {
            // Arrange

            var expected = GetTestProducts();
            var mockProducts = new Mock<IProductsRepository>(MockBehavior.Strict);
            mockProducts.Setup(r => r.GetAllProductsAsync())
                .ReturnsAsync(expected)
                .Verifiable();
            var controller = new ProductController(mockProducts.Object, _mapper, _memoryCacheMock.Object, _memoryCacheModel);

            // Act
            var result = await controller.GetAllProducts();

            // Assert
            var viewResult = Assert.IsType<OkObjectResult>(result.Result);
            var model = Assert.IsAssignableFrom<IEnumerable<ProductReadDTO>>(
                viewResult.Value);
            model.Should().BeEquivalentTo(expected);
            mockProducts.Verify(r => r.GetAllProductsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAll_WhenCalled_ReturnsOkResult()
        {
            // Arrange

            var expected = GetTestProducts();
            var mockProducts = new Mock<IProductsRepository>(MockBehavior.Strict);
            mockProducts.Setup(r => r.GetAllProductsAsync())
                .ReturnsAsync(expected)
                .Verifiable();
            var controller = new ProductController(mockProducts.Object, _mapper, _memoryCacheMock.Object, _memoryCacheModel);

            // Act
            var result = await controller.GetAllProducts();

            // Assert
            var viewResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetAll_WhenCalledWithNoProducts_ReturnsNoItems()
        {
            // Arrange
            var expected = new ProductDomainModel[] { };
            var mockProducts = new Mock<IProductsRepository>(MockBehavior.Strict);
            mockProducts.Setup(r => r.GetAllProductsAsync())
                .ReturnsAsync(expected)
                .Verifiable();
            var controller = new ProductController(mockProducts.Object, _mapper, _memoryCacheMock.Object, _memoryCacheModel);

            // Act
            var result = await controller.GetAllProducts();

            // Assert
            var viewResult = Assert.IsType<OkObjectResult>(result.Result);
            var model = Assert.IsAssignableFrom<IEnumerable<ProductReadDTO>>(
                viewResult.Value);
            Assert.Equal(expected.Length, model.Count());
            mockProducts.Verify(r => r.GetAllProductsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAll_WhenBadServiceCall_ShouldInternalError()
        {
            // Arrange
            var mockProducts = new Mock<IProductsRepository>(MockBehavior.Strict);
            mockProducts.Setup(r => r.GetAllProductsAsync())
                .ThrowsAsync(new Exception())
                .Verifiable();
            var controller = new ProductController(mockProducts.Object, _mapper, _memoryCacheMock.Object, _memoryCacheModel);

            await Assert.ThrowsAsync<Exception>(async () => await controller.GetAllProducts());
        }

        [Fact]
        public async Task GetOne_WhenCalled_ReturnsItem()
        {
            // Arrange

            var expected = new ProductDomainModel { ProductID = 2, ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 };
            var mockProducts = new Mock<IProductsRepository>(MockBehavior.Strict);
            mockProducts.Setup(r => r.GetProductAsync(2))
                        .ReturnsAsync(expected)
                        .Verifiable();

            var controller = new ProductController(mockProducts.Object, _mapper, _memoryCacheMock.Object, _memoryCacheModel);

            // Act
            var result = await controller.GetProduct(2);

            // Assert
            var viewResult = Assert.IsType<OkObjectResult>(result.Result);
            var model = Assert.IsAssignableFrom<ProductReadDTO>(
                viewResult.Value);
            mockProducts.Verify(r => r.GetProductAsync(2), Times.Once);
        }

        [Fact]
        public async Task GetOne_WhenCalled_ReturnsCorrectItem()
        {
            // Arrange

            var expected = new ProductDomainModel { ProductID = 2, ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 };
            var mockProducts = new Mock<IProductsRepository>(MockBehavior.Strict);
            mockProducts.Setup(r => r.GetProductAsync(2))
                        .ReturnsAsync(expected)
                        .Verifiable();

            var controller = new ProductController(mockProducts.Object, _mapper, _memoryCacheMock.Object, _memoryCacheModel);

            // Act
            var result = await controller.GetProduct(2);

            // Assert
            var viewResult = Assert.IsType<OkObjectResult>(result.Result);
            var model = Assert.IsAssignableFrom<ProductReadDTO>(
                viewResult.Value);

            model.Should().BeEquivalentTo(expected);
            mockProducts.Verify(r => r.GetProductAsync(2), Times.Once);
        }

        [Fact]
        public async Task GetOne_WhenCalled_ReturnsOkResult()
        {
            // Arrange

            var expected = new ProductDomainModel { ProductID = 2, ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 };
            var mockProducts = new Mock<IProductsRepository>(MockBehavior.Strict);
            mockProducts.Setup(r => r.GetProductAsync(2))
                        .ReturnsAsync(expected)
                        .Verifiable();

            var controller = new ProductController(mockProducts.Object, _mapper, _memoryCacheMock.Object, _memoryCacheModel);

            // Act
            var result = await controller.GetProduct(2);

            // Assert
            var viewResult = Assert.IsType<OkObjectResult>(result.Result);
            mockProducts.Verify(r => r.GetProductAsync(2), Times.Once);
        }

        [Fact]
        public async Task GetOne_WhenCalledWithNoProducts_ThrowsNotFound()
        {
            // Arrange

            var expected = new ProductDomainModel { ProductID = 2, ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 };
            var mockProducts = new Mock<IProductsRepository>(MockBehavior.Strict);
            mockProducts.Setup(r => r.GetProductAsync(2))
                        .ReturnsAsync((ProductDomainModel)null)
                        .Verifiable();

            var controller = new ProductController(mockProducts.Object, _mapper, _memoryCacheMock.Object, _memoryCacheModel);

            // Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(async () => await controller.GetProduct(2));
        }

        [Fact]
        public async Task GetOne_WhenBadServiceCall_ShouldInternalError()
        {
            // Arrange

            var expected = new ProductDomainModel { ProductID = 2, ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 };
            var mockProducts = new Mock<IProductsRepository>(MockBehavior.Strict);
            mockProducts.Setup(r => r.GetProductAsync(2))
                        .ThrowsAsync(new Exception())
                        .Verifiable();

            var controller = new ProductController(mockProducts.Object, _mapper, _memoryCacheMock.Object, _memoryCacheModel);

            // Assert
            await Assert.ThrowsAsync<Exception>(async () => await controller.GetProduct(2));
        }

        [Fact]
        public async Task Create_WhenCalled_CreatesItem()
        {
            // Arrange

            var createDTO = new ProductCreateDTO { ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 };
            var expected = new ProductDomainModel { ProductID = 2, ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 };
            var mockProducts = new Mock<IProductsRepository>(MockBehavior.Strict);
            mockProducts.Setup(r => r.CreateProduct(It.IsAny<ProductDomainModel>()))
                        .Returns(2)
                        .Verifiable();

            mockProducts.Setup(r => r.SaveChangesAsync())
                        .Returns(Task.CompletedTask)
                        .Verifiable();

            var controller = new ProductController(mockProducts.Object, _mapper, _memoryCacheMock.Object, _memoryCacheModel);

            // Act
            var result = await controller.CreateProduct(createDTO);

            // Assert
            var viewResult = Assert.IsType<CreatedAtActionResult>(result);
            var model = Assert.IsAssignableFrom<ProductDomainModel>(
                viewResult.Value);
            mockProducts.Verify(r => r.CreateProduct(It.IsAny<ProductDomainModel>()), Times.Once);
            mockProducts.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Create_WhenCalled_CreatesCorrectItem()
        {
            // Arrange

            var createDTO = new ProductCreateDTO { ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 };
            var expected = new ProductDomainModel { ProductID = 2, ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 };
            var expectedAfterCreation = new ProductDomainModel { ProductID = 0, ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 };
            var mockProducts = new Mock<IProductsRepository>(MockBehavior.Strict);
            mockProducts.Setup(r => r.CreateProduct(It.IsAny<ProductDomainModel>()))
                        .Returns(2)
                        .Verifiable();

            mockProducts.Setup(r => r.SaveChangesAsync())
                        .Returns(Task.CompletedTask)
                        .Verifiable();

            var controller = new ProductController(mockProducts.Object, _mapper, _memoryCacheMock.Object, _memoryCacheModel);

            // Act
            var result = await controller.CreateProduct(createDTO);

            // Assert
            var viewResult = Assert.IsType<CreatedAtActionResult>(result);
            var model = Assert.IsAssignableFrom<ProductDomainModel>(
                viewResult.Value);

            model.Should().BeEquivalentTo(expectedAfterCreation);

            mockProducts.Verify(r => r.CreateProduct(It.IsAny<ProductDomainModel>()), Times.Once);
            mockProducts.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Create_WhenCalled_ReturnsCreatedAtActionResult()
        {
            // Arrange

            var createDTO = new ProductCreateDTO { ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 };
            var expected = new ProductDomainModel { ProductID = 2, ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 };
            var mockProducts = new Mock<IProductsRepository>(MockBehavior.Strict);
            mockProducts.Setup(r => r.CreateProduct(It.IsAny<ProductDomainModel>()))
                        .Returns(2)
                        .Verifiable();

            mockProducts.Setup(r => r.SaveChangesAsync())
                        .Returns(Task.CompletedTask)
                        .Verifiable();

            var controller = new ProductController(mockProducts.Object, _mapper, _memoryCacheMock.Object, _memoryCacheModel);

            // Act
            var result = await controller.CreateProduct(createDTO);

            // Assert
            var viewResult = Assert.IsType<CreatedAtActionResult>(result);

            mockProducts.Verify(r => r.CreateProduct(It.IsAny<ProductDomainModel>()), Times.Once);
            mockProducts.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Create_WhenCalled_ReturnsCorrectID()
        {
            // Arrange

            var createDTO = new ProductCreateDTO { ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 };
            var expected = new ProductDomainModel { ProductID = 2, ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 };
            var mockProducts = new Mock<IProductsRepository>(MockBehavior.Strict);
            mockProducts.Setup(r => r.CreateProduct(It.IsAny<ProductDomainModel>()))
                        .Returns(2)
                        .Verifiable();

            mockProducts.Setup(r => r.SaveChangesAsync())
                        .Returns(Task.CompletedTask)
                        .Verifiable();

            var controller = new ProductController(mockProducts.Object, _mapper, _memoryCacheMock.Object, _memoryCacheModel);

            // Act
            var result = await controller.CreateProduct(createDTO);

            // Assert
            var viewResult = Assert.IsType<CreatedAtActionResult>(result);

            var resultRouteValue = Assert.IsType<RouteValueDictionary>(viewResult.RouteValues);

            var resultID = Assert.IsType<int>(resultRouteValue.FirstOrDefault().Value);

            Assert.Equal(2, resultID);

            mockProducts.Verify(r => r.CreateProduct(It.IsAny<ProductDomainModel>()), Times.Once);
            mockProducts.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Create_WhenCalled_ReturnsCorrectActionName()
        {
            // Arrange

            var createDTO = new ProductCreateDTO { ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 };
            var expected = new ProductDomainModel { ProductID = 2, ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 };
            var mockProducts = new Mock<IProductsRepository>(MockBehavior.Strict);
            mockProducts.Setup(r => r.CreateProduct(It.IsAny<ProductDomainModel>()))
                        .Returns(2)
                        .Verifiable();

            mockProducts.Setup(r => r.SaveChangesAsync())
                        .Returns(Task.CompletedTask)
                        .Verifiable();

            var controller = new ProductController(mockProducts.Object, _mapper, _memoryCacheMock.Object, _memoryCacheModel);

            // Act
            var result = await controller.CreateProduct(createDTO);

            // Assert
            var viewResult = Assert.IsType<CreatedAtActionResult>(result);

            var resultActionName = Assert.IsType<string>(viewResult.ActionName);

            Assert.Equal("GetProduct", resultActionName);

            mockProducts.Verify(r => r.CreateProduct(It.IsAny<ProductDomainModel>()), Times.Once);
            mockProducts.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Create_WhenBadServiceCall_ShouldInternalError()
        {
            // Arrange
            var createDTO = new ProductCreateDTO { ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 };
            var mockProducts = new Mock<IProductsRepository>(MockBehavior.Strict);
            mockProducts.Setup(r => r.CreateProduct(It.IsAny<ProductDomainModel>()))
                        .Throws(new Exception())
                        .Verifiable();

            mockProducts.Setup(r => r.SaveChangesAsync())
                        .Returns(Task.CompletedTask)
                        .Verifiable();
            var controller = new ProductController(mockProducts.Object, _mapper, _memoryCacheMock.Object, _memoryCacheModel);

            await Assert.ThrowsAsync<Exception>(async () => await controller.CreateProduct(createDTO));
        }

        [Fact]
        public async Task Update_WhenCalled_UpdatesItem()
        {
            // Arrange

            var updateDto = new ProductEditDTO { ProductName = "Updated Bread", ProductDescription = "Updated Fresh.", ProductPrice = 99.56, ProductQuantity = 9999 };
            JsonPatchDocument<ProductEditDTO> jsonPatchDocument = new JsonPatchDocument<ProductEditDTO>();
            jsonPatchDocument.Replace<int>(jp => jp.ProductQuantity, updateDto.ProductQuantity);
            var originalModel = new ProductDomainModel { ProductID = 2, ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 };
            var mockProducts = new Mock<IProductsRepository>(MockBehavior.Strict);
            mockProducts.Setup(r => r.UpdateProduct(It.IsAny<ProductDomainModel>()))
                        .Verifiable();

            mockProducts.Setup(r => r.SaveChangesAsync())
                        .Returns(Task.CompletedTask)
                        .Verifiable();

            mockProducts.Setup(r => r.GetProductAsync(2))
                        .ReturnsAsync(originalModel)
                        .Verifiable();

            var objectValidator = new Mock<IObjectModelValidator>();
            objectValidator.Setup(o => o.Validate(It.IsAny<ActionContext>(),
                                              It.IsAny<ValidationStateDictionary>(),
                                              It.IsAny<string>(),
                                              It.IsAny<Object>()));

            var controller = new ProductController(mockProducts.Object, _mapper, _memoryCacheMock.Object, _memoryCacheModel);

            controller.ObjectValidator = objectValidator.Object;

            // Act
            var result = await controller.UpdateProduct(2, jsonPatchDocument);

            // Assert
            var viewResult = Assert.IsType<OkResult>(result);
            mockProducts.Verify(r => r.GetProductAsync(It.IsAny<int>()), Times.Once);
            mockProducts.Verify(r => r.UpdateProduct(It.IsAny<ProductDomainModel>()), Times.Once);
            mockProducts.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Update_WhenBadServiceCall_ShouldInternalError()
        {
            // Arrange

            var updateDto = new ProductEditDTO { ProductName = "Updated Bread", ProductDescription = "Updated Fresh.", ProductPrice = 99.56, ProductQuantity = 9999 };
            JsonPatchDocument<ProductEditDTO> jsonPatchDocument = new JsonPatchDocument<ProductEditDTO>();
            jsonPatchDocument.Replace<int>(jp => jp.ProductQuantity, updateDto.ProductQuantity);
            var originalModel = new ProductDomainModel { ProductID = 2, ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 };
            var mockProducts = new Mock<IProductsRepository>(MockBehavior.Strict);
            mockProducts.Setup(r => r.UpdateProduct(It.IsAny<ProductDomainModel>()))
                        .Throws(new Exception())
                        .Verifiable();

            mockProducts.Setup(r => r.GetProductAsync(2))
                        .ReturnsAsync(originalModel)
                        .Verifiable();

            var objectValidator = new Mock<IObjectModelValidator>();
            objectValidator.Setup(o => o.Validate(It.IsAny<ActionContext>(),
                                              It.IsAny<ValidationStateDictionary>(),
                                              It.IsAny<string>(),
                                              It.IsAny<Object>()));

            var controller = new ProductController(mockProducts.Object, _mapper, _memoryCacheMock.Object, _memoryCacheModel);

            controller.ObjectValidator = objectValidator.Object;

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await controller.UpdateProduct(2, jsonPatchDocument));

            mockProducts.Verify(r => r.GetProductAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task Update_WhenCalledWithWrongID_ThrowsNotFound()
        {
            // Arrange

            var updateDto = new ProductEditDTO { ProductName = "Updated Bread", ProductDescription = "Updated Fresh.", ProductPrice = 99.56, ProductQuantity = 9999 };
            JsonPatchDocument<ProductEditDTO> jsonPatchDocument = new JsonPatchDocument<ProductEditDTO>();
            jsonPatchDocument.Replace<int>(jp => jp.ProductQuantity, updateDto.ProductQuantity);
            var mockProducts = new Mock<IProductsRepository>(MockBehavior.Strict);

            mockProducts.Setup(r => r.GetProductAsync(2))
                        .ReturnsAsync((ProductDomainModel)null)
                        .Verifiable();

            var objectValidator = new Mock<IObjectModelValidator>();
            objectValidator.Setup(o => o.Validate(It.IsAny<ActionContext>(),
                                              It.IsAny<ValidationStateDictionary>(),
                                              It.IsAny<string>(),
                                              It.IsAny<Object>()));

            var controller = new ProductController(mockProducts.Object, _mapper, _memoryCacheMock.Object, _memoryCacheModel);

            controller.ObjectValidator = objectValidator.Object;

            // Act
            await Assert.ThrowsAsync<ResourceNotFoundException>(async () => await controller.UpdateProduct(2, jsonPatchDocument));

            mockProducts.Verify(r => r.GetProductAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task Update_WhenCalledWithModelStateError_ThrowsArgumentException()
        {
            // Arrange

            var updateDto = new ProductEditDTO { ProductName = "Updated Bread", ProductDescription = "Updated Fresh.", ProductPrice = 99.56, ProductQuantity = 9999 };
            JsonPatchDocument<ProductEditDTO> jsonPatchDocument = new JsonPatchDocument<ProductEditDTO>();
            jsonPatchDocument.Replace<int>(jp => jp.ProductQuantity, updateDto.ProductQuantity);
            var originalModel = new ProductDomainModel { ProductID = 2, ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 };
            var mockProducts = new Mock<IProductsRepository>(MockBehavior.Strict);

            mockProducts.Setup(r => r.GetProductAsync(2))
                        .ReturnsAsync(originalModel)
                        .Verifiable();

            var controller = new ProductController(mockProducts.Object, _mapper, _memoryCacheMock.Object, _memoryCacheModel);

            controller.ObjectValidator = new FaultyValidator();

            // Act
            await Assert.ThrowsAsync<ArgumentException>(async () => await controller.UpdateProduct(2, jsonPatchDocument));

            // Assert
            mockProducts.Verify(r => r.GetProductAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task Delete_WhenCalled_DeletesItem()
        {
            // Arrange
            var originalModel = new ProductDomainModel { ProductID = 2, ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 };
            var mockProducts = new Mock<IProductsRepository>(MockBehavior.Strict);
            mockProducts.Setup(r => r.DeleteProductAsync(It.IsAny<int>()))
                        .ReturnsAsync(originalModel)
                        .Verifiable();

            mockProducts.Setup(r => r.SaveChangesAsync())
                        .Returns(Task.CompletedTask)
                        .Verifiable();

            mockProducts.Setup(r => r.GetProductAsync(2))
                        .ReturnsAsync(originalModel)
                        .Verifiable();


            var controller = new ProductController(mockProducts.Object, _mapper, _memoryCacheMock.Object, _memoryCacheModel);

            // Act
            var result = await controller.DeleteProduct(2);

            // Assert
            var viewResult = Assert.IsType<NoContentResult>(result.Result);

            mockProducts.Verify(r => r.GetProductAsync(It.IsAny<int>()), Times.Once);
            mockProducts.Verify(r => r.DeleteProductAsync(It.IsAny<int>()), Times.Once);
            mockProducts.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Delete_WhenBadServiceCall_ShouldInternalError()
        {
            // Arrange
            var originalModel = new ProductDomainModel { ProductID = 2, ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 };
            var mockProducts = new Mock<IProductsRepository>(MockBehavior.Strict);
            mockProducts.Setup(r => r.DeleteProductAsync(It.IsAny<int>()))
                        .Throws(new Exception())
                        .Verifiable();

            mockProducts.Setup(r => r.SaveChangesAsync())
                        .Returns(Task.CompletedTask)
                        .Verifiable();

            mockProducts.Setup(r => r.GetProductAsync(2))
                        .ReturnsAsync(originalModel)
                        .Verifiable();

            var controller = new ProductController(mockProducts.Object, _mapper, _memoryCacheMock.Object, _memoryCacheModel);

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await controller.DeleteProduct(2));

            mockProducts.Verify(r => r.GetProductAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task Delete_WhenCalledWithWrongID_ThrowsNotFound()
        {
            // Arrange
            var originalModel = new ProductDomainModel { ProductID = 2, ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 };
            var mockProducts = new Mock<IProductsRepository>(MockBehavior.Strict);
            mockProducts.Setup(r => r.DeleteProductAsync(It.IsAny<int>()))
                        .ReturnsAsync(originalModel)
                        .Verifiable();

            mockProducts.Setup(r => r.SaveChangesAsync())
                        .Returns(Task.CompletedTask)
                        .Verifiable();

            mockProducts.Setup(r => r.GetProductAsync(2))
                        .ReturnsAsync((ProductDomainModel)null)
                        .Verifiable();

            var controller = new ProductController(mockProducts.Object, _mapper, _memoryCacheMock.Object, _memoryCacheModel);

            // Act
            await Assert.ThrowsAsync<ResourceNotFoundException>(async () => await controller.DeleteProduct(2));

            mockProducts.Verify(r => r.GetProductAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task AddStock_WhenCalled_AddsStock()
        {
            // Arrange

            var updateDto = new ProductAddStockDTO { ProductQuantityToAdd = 45 };
            var originalModel = new ProductDomainModel { ProductID = 2, ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 };
            var mockProducts = new Mock<IProductsRepository>(MockBehavior.Strict);
            mockProducts.Setup(r => r.AddStock(It.IsAny<int>(), It.IsAny<int>()))
                        .Verifiable();

            mockProducts.Setup(r => r.SaveChangesAsync())
                        .Returns(Task.CompletedTask)
                        .Verifiable();

            mockProducts.Setup(r => r.GetProductAsync(2))
                        .ReturnsAsync(originalModel)
                        .Verifiable();

            var controller = new ProductController(mockProducts.Object, _mapper, _memoryCacheMock.Object, _memoryCacheModel);

            // Act
            var result = await controller.AddStock(2, updateDto);

            // Assert
            var viewResult = Assert.IsType<OkResult>(result);

            mockProducts.Verify(r => r.GetProductAsync(It.IsAny<int>()), Times.Once);
            mockProducts.Verify(r => r.AddStock(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockProducts.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddStock_WhenBadServiceCall_ShouldInternalError()
        {
            // Arrange

            var updateDto = new ProductAddStockDTO { ProductQuantityToAdd = 45 };
            var originalModel = new ProductDomainModel { ProductID = 2, ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 };
            var mockProducts = new Mock<IProductsRepository>(MockBehavior.Strict);
            mockProducts.Setup(r => r.AddStock(It.IsAny<int>(), It.IsAny<int>()))
                        .Throws(new Exception())
                        .Verifiable();

            mockProducts.Setup(r => r.GetProductAsync(2))
                        .ReturnsAsync(originalModel)
                        .Verifiable();

            var controller = new ProductController(mockProducts.Object, _mapper, _memoryCacheMock.Object, _memoryCacheModel);

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await controller.AddStock(2, updateDto));

            mockProducts.Verify(r => r.GetProductAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task AddStock_WhenCalledWithWrongID_ThrowsNotFound()
        {
            // Arrange

            var updateDto = new ProductAddStockDTO { ProductQuantityToAdd = 45 };
            var originalModel = new ProductDomainModel { ProductID = 2, ProductName = "Bread", ProductDescription = "Fresh.", ProductPrice = 0.56, ProductQuantity = 1243 };
            var mockProducts = new Mock<IProductsRepository>(MockBehavior.Strict);

            mockProducts.Setup(r => r.GetProductAsync(2))
                        .ReturnsAsync((ProductDomainModel) null)
                        .Verifiable();

            var controller = new ProductController(mockProducts.Object, _mapper, _memoryCacheMock.Object, _memoryCacheModel);

            // Act
            await Assert.ThrowsAsync<ResourceNotFoundException>(async () => await controller.AddStock(2, updateDto));

            mockProducts.Verify(r => r.GetProductAsync(It.IsAny<int>()), Times.Once);
        }
    }
}
