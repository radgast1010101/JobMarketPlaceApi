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

            var repoMock2 = new Mock<IJobOfferRepository>();

            repoMock2
                .Setup(r => r.CreateAsync(jobOfferResults, It.IsAny<CancellationToken>()))
                .ReturnsAsync(jobOfferResults)
                .Verifiable();

            //repoMock2.SetupAllProperties();

            var svc2 = new ContractorJobOfferService(repoMock2.Object); 
            
            // Act
            var result2 = await svc2.CreateOfferAsync(contractorId, jobId, price);

            // Assert
            result2.Should().NotBeNull();
            result2.ContractorId.Equals(contractorId);
            result2.JobId.Equals(jobId);
            result2.Price.Equals(price);
            result2.Id.Equals(jobOfferId);
        }
    }
}