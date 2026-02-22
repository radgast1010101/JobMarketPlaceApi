using FluentAssertions;
using JobMarketPlaceApi.Data.Repositories;
using JobMarketPlaceApi.Domain;
using JobMarketPlaceApi.Entities;
using JobMarketPlaceApi.Services;
using Moq;
using Xunit;

namespace JobMarketPlaceApi.Tests.Domain
{
    public class ContractorDomainTests
    {

        [Fact]
        public async Task CreateJobOffer_HasResults()
        {
            // Arrange
            var contractorId = Guid.NewGuid();
            var jobId = Guid.NewGuid();
            var jobOfferId = Guid.NewGuid();

            var price = 100;
            var jobOfferResults = new JobOffer 
            { 
                Id = jobOfferId, 
                JobId = jobId, 
                ContractorId = contractorId, 
                Price = price 
            };

            var repoMock = new Mock<IJobOfferRepository>(MockBehavior.Strict);

            repoMock
                .Setup(r => r.CreateAsync(
                    It.Is<JobOffer>(jo =>
                        jo.JobId == jobId &&
                        jo.ContractorId == contractorId &&
                        jo.Price == price),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(jobOfferResults)
                .Verifiable();

            var svc = new ContractorJobOfferService(repoMock.Object); 
            
            // Act
            var result = await svc.CreateOfferAsync(contractorId, jobId, price);

            // Assert
            repoMock.Verify(); // ensure repository was called
            result.Should().NotBeNull();
            result.ContractorId.Should().Be(contractorId);
            result.JobId.Should().Be(jobId);
            result.Price.Should().Be(price);
            result.Id.Should().Be(jobOfferId);
        }
    }
}