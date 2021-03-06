﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Moq;
using NUnit.Framework;
using SFB.Web.Common;
using SFB.Web.Domain.Models;
using SFB.Web.Domain.Services.Comparison;
using SFB.Web.Domain.Services.DataAccess;
using SFB.Web.UI.Services;

namespace SFB.Web.UI.UnitTests
{
    public class ComparisonServiceUnitTests
    {
        [Test]
        public async Task GenerateBenchmarkListWithSimpleComparisonAsyncShouldExpandTheUrbanRuralIfNotEnoughSchoolsFound()
        {
            var mockFinancialDataService = new Mock<IFinancialDataService>();
            var testResult = new Document();
            testResult.SetPropertyValue("URN", "321");
            testResult.SetPropertyValue("School Name", "test");
            testResult.SetPropertyValue("FinanceType", "A");
            testResult.SetPropertyValue("UrbanRuralInner", "Town and fringe");
            Task<List<Document>> task = Task.Run(() =>
            {
                return new List<Document> { testResult };
            });

            mockFinancialDataService.Setup(m => m.SearchSchoolsByCriteriaAsync(It.IsAny<BenchmarkCriteria>(), It.IsAny<EstablishmentType>()))
                .Returns((BenchmarkCriteria criteria, EstablishmentType estType) => task);

            var mockBenchmarkCriteriaBuilderService = new Mock<IBenchmarkCriteriaBuilderService>();
            mockBenchmarkCriteriaBuilderService.Setup(s => s.BuildFromSimpleComparisonCriteria(It.IsAny<SchoolFinancialDataModel>(), It.IsAny<SimpleCriteria>(), It.IsAny<int>()))
                .Returns((SchoolFinancialDataModel dm, SimpleCriteria sc, int percentage) => new BenchmarkCriteria() { Gender = new[] { "Male" } });

            var service = new ComparisonService(mockFinancialDataService.Object, mockBenchmarkCriteriaBuilderService.Object);

            var comparisonResult = await service.GenerateBenchmarkListWithSimpleComparisonAsync(new BenchmarkCriteria(){ Gender = new []{"Male"}},
                EstablishmentType.Maintained, 15, new SimpleCriteria(), new SchoolFinancialDataModel("123","14-15",testResult,SchoolFinancialType.Maintained));

            mockFinancialDataService.Verify(s => s.SearchSchoolsByCriteriaAsync(It.IsAny<BenchmarkCriteria>(), EstablishmentType.Maintained), Times.AtLeast(11));
            Assert.AreEqual(5, comparisonResult.BenchmarkCriteria.UrbanRural.Length);
            Assert.IsTrue(comparisonResult.BenchmarkCriteria.UrbanRural.Contains("Rural and village"));
            Assert.IsTrue(comparisonResult.BenchmarkCriteria.UrbanRural.Contains("Town and fringe"));
            Assert.IsTrue(comparisonResult.BenchmarkCriteria.UrbanRural.Contains("Urban and city"));
            Assert.IsTrue(comparisonResult.BenchmarkCriteria.UrbanRural.Contains("Hamlet and isolated dwelling"));
            Assert.IsTrue(comparisonResult.BenchmarkCriteria.UrbanRural.Contains("Conurbation"));
        }
    }
}