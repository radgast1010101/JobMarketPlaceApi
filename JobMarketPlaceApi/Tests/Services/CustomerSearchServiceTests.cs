// JobMarketPlaceApi.Tests\Services\CustomerSearchServiceTests.cs
using FluentAssertions;
using Moq;
//using System;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;
using JobMarketPlaceApi.Data.Repositories;
using JobMarketPlaceApi.Entities;
using JobMarketPlaceApi.Services;
using Xunit;

namespace JobMarketPlaceApi.Tests.Services
{
    public class CustomerSearchServiceTests
    {
        [Fact]
        public async Task SearchByLastNamePrefixAsync_PrefixTooShort_ThrowsArgumentException()
        {
            // Arrange
            var repoMock = new Mock<ICustomerRepository>(MockBehavior.Strict);
            var svc = new CustomerSearchService(repoMock.Object);

            // Act
            Func<Task> act = async () => await svc.SearchByLastNamePrefixAsync("ab", 1, 20);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*prefix must be at least*");
        }

        [Fact]
        public async Task SearchByLastNamePrefixAsync_PageSizeTooLarge_ClampsAndReturnsHasMore()
        {
            // Arrange
            var prefix = "smi";
            var requestedSize = 200; // larger than MaxPageSize (100)
            var expectedClampedSize = 100;
            var pageNumber = 1;

            // prepare repository result to simulate a page with HasMore = true
            var sampleCustomers = new List<Customer>();
            for (int i = 0; i < expectedClampedSize; i++)
            {
                sampleCustomers.Add(new Customer { Id = Guid.NewGuid(), FirstName = $"F{i}", LastName = $"Smith{i}" });
            }

            var repoResult = new SearchResult<Customer>(sampleCustomers, true, null, null);

            var repoMock = new Mock<ICustomerRepository>(MockBehavior.Strict);
            repoMock
                .Setup(r => r.SearchByLastNamePrefixAsync(prefix, pageNumber, expectedClampedSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync(repoResult)
                .Verifiable();

            var svc = new CustomerSearchService(repoMock.Object);

            // Act
            var result = await svc.SearchByLastNamePrefixAsync(prefix, pageNumber, requestedSize);

            // Assert
            result.Should().NotBeNull();
            result.HasMore.Should().BeTrue();
            result.Items.Should().HaveCount(expectedClampedSize);
            repoMock.Verify(); // ensure repository was called with clamped page size
        }
    }
}