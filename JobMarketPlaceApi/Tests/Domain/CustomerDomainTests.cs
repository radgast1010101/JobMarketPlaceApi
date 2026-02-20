// JobMarketPlaceApi.Tests\Domain\CustomerDomainTests.cs
//using System;
using FluentAssertions;
using JobMarketPlaceApi.Domain;
using JobMarketPlaceApi.Entities;
using Xunit;

namespace JobMarketPlaceApi.Tests.Domain
{
    public class CustomerDomainTests
    {
        [Fact]
        public void DomainException_Is_Exception()
        {
            // Arrange / Act
            var ex = new DomainException("boom");

            // Assert
            ex.Should().BeOfType<DomainException>();
            ex.Should().BeAssignableTo<Exception>();
            ex.Message.Should().Be("boom");
        }

        [Fact]
        public void CreateJob_WithEmptyDescription_ThrowsDomainException()
        {
            // Arrange
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            Action act = () => customer.CreateJob(string.Empty, DateTime.UtcNow);

            // Assert
            act.Should().Throw<DomainException>()
               .WithMessage("*Description is required*");
        }

        [Fact]
        public void CreateJob_StartAfterDue_ThrowsDomainException()
        {
            // Arrange
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = "Alice",
                LastName = "Smith"
            };

            var start = new DateTime(2026, 1, 10);
            var due = new DateTime(2026, 1, 1); // earlier than start

            // Act
            Action act = () => customer.CreateJob("Work", start, due);

            // Assert
            act.Should().Throw<DomainException>()
               .WithMessage("*StartDate must be less than or equal to DueDate*");
        }

        [Fact]
        public void CreateJob_Nulls_DefaultsAndReturnsJob()
        {
            // Arrange
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = "Bob",
                LastName = "Painter"
            };

            var start = DateTime.UtcNow;
            string descriptionWithWhitespace = "  Paint the fence  ";

            // Act
            var job = customer.CreateJob(descriptionWithWhitespace, start, null, null);

            // Assert
            job.Should().NotBeNull();
            job.Id.Should().NotBe(Guid.Empty);
            job.CustomerId.Should().Be(customer.Id);
            job.Description.Should().Be("Paint the fence"); // trimmed
            job.StartDate.Should().Be(start);
            job.DueDate.Should().Be(start); // defaulted to start
            job.Budget.Should().Be(0); // defaulted
            job.AcceptedBy.Should().BeEmpty();
        }
    }
}