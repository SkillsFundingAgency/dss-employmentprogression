using System;
using Moq;
using NCS.DSS.EmploymentProgression.Cosmos.Provider;
using NCS.DSS.EmploymentProgression.PostEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.ReferenceData;
using NCS.DSS.EmploymentProgression.ServiceBus;
using NUnit.Framework;

namespace NCS.DSS.EmploymentProgression.Tests.ServiceTests;

public class EmploymentProgressionPostTriggerServiceTests
{
    private EmploymentProgressionPostTriggerService _sut;

    [SetUp]
    public void Setup()
    {
        _sut = new EmploymentProgressionPostTriggerService(new Mock<IDocumentDBProvider>().Object,
            new Mock<IServiceBusClient>().Object);
    }

    [Test]
    [TestCase(EconomicShockStatus.GovernmentDefinedEconomicShock, EconomicShockStatus.GovernmentDefinedEconomicShock)]
    [TestCase(EconomicShockStatus.LocalEconomicShock, EconomicShockStatus.LocalEconomicShock)]
    [TestCase(EconomicShockStatus.NotApplicable, EconomicShockStatus.NotApplicable)]
    [TestCase(null, EconomicShockStatus.NotApplicable)]
    public void SetDefaults_WithValues_SetsExpectedEconomicShockStatusValue(EconomicShockStatus? status,
        EconomicShockStatus expected)
    {
        // Arrange
        var employmentProgression = new Models.EmploymentProgression
        {
            EconomicShockStatus = status
        };

        // Act
        _sut.SetDefaults(employmentProgression, "ANY_STRING");

        // Assert
        Assert.That(employmentProgression.EconomicShockStatus,Is.EqualTo(expected));
    }

    [Test]
    public void SetDefaults_WithValues_SetsExpectedCreatedBy()
    {
        // Arrange
        var expected = "100000000";
        var employmentProgression = new Models.EmploymentProgression();

        // Act
        _sut.SetDefaults(employmentProgression, expected);

        // Assert
        Assert.That(employmentProgression.CreatedBy, Is.EqualTo(expected));
    }

    [Test]
    public void SetDefaults_WithNullValues_SetsExpectedDateProgressionRecorded()
    {
        // Arrange
        var employmentProgression = new Models.EmploymentProgression
        {
            DateProgressionRecorded = null
        };

        // Act
        _sut.SetDefaults(employmentProgression, "ANY_STRING");

        // Assert
        Assert.That(employmentProgression.DateProgressionRecorded,Is.Not.Null);
    }

    [Test]
    public void SetDefaults_WithValues_DoesNotSetExpectedDateProgressionRecorded()
    {
        // Arrange
        var expected = new DateTime(2000, 1, 1);
        var employmentProgression = new Models.EmploymentProgression
        {
            DateProgressionRecorded = expected
        };

        // Act
        _sut.SetDefaults(employmentProgression, "ANY_STRING");

        // Assert
        Assert.That(employmentProgression.DateProgressionRecorded, Is.EqualTo(expected));
    }
}