﻿using System;
using System.Linq;
using System.Threading.Tasks;
using SFB.Web.Common;
using SFB.Web.Domain.Helpers.Constants;
using SFB.Web.Domain.Models;
using SFB.Web.Domain.Services.DataAccess;

namespace SFB.Web.Domain.Services.Comparison
{
    public class ComparisonService : IComparisonService
    {
        private readonly IFinancialDataService _financialDataService;
        private readonly IBenchmarkCriteriaBuilderService _benchmarkCriteriaBuilderService;

        public ComparisonService(IFinancialDataService financialDataService, IBenchmarkCriteriaBuilderService benchmarkCriteriaBuilderService)
        {
            _financialDataService = financialDataService;
            _benchmarkCriteriaBuilderService = benchmarkCriteriaBuilderService;
        }

        public async Task<ComparisonResult> GenerateBenchmarkListWithAdvancedComparisonAsync(BenchmarkCriteria criteria, EstablishmentType estType, int basketSize = ComparisonListLimit.LIMIT)
        {
            var benchmarkSchools = await _financialDataService.SearchSchoolsByCriteriaAsync(criteria, estType);
            var limitedList = benchmarkSchools.Take(basketSize).ToList();
            return new ComparisonResult()
            {
                BenchmarkSchools = limitedList,
                BenchmarkCriteria = criteria
            };
        }

        public async Task<ComparisonResult> GenerateBenchmarkListWithSimpleComparisonAsync(
            BenchmarkCriteria benchmarkCriteria, EstablishmentType estType,
            int basketSize,
            SimpleCriteria simpleCriteria, SchoolFinancialDataModel defaultSchoolFinancialDataModel)
        {
            var benchmarkSchools = await _financialDataService.SearchSchoolsByCriteriaAsync(benchmarkCriteria, estType);

            if (benchmarkSchools.Count > basketSize) //Original query returns more than required. Cut from top by proximity.
            {
                benchmarkSchools = benchmarkSchools.OrderBy(b => Math.Abs(b.GetPropertyValue<int>("No Pupils") - defaultSchoolFinancialDataModel.PupilCount)).Take(basketSize).ToList();
                benchmarkCriteria.MinNoPupil = benchmarkSchools.Min(s => s.GetPropertyValue<int>("No Pupils"));
                benchmarkCriteria.MaxNoPupil = benchmarkSchools.Max(s => s.GetPropertyValue<int>("No Pupils")); //Update the criteria to reflect the max and min pupil count of the found schools
            }

            var tryCount = 0;
            while (benchmarkSchools.Count < basketSize) //Original query returns less than required
            {
                if (++tryCount > CriteriaSearchConfig.MAX_TRY_LIMIT) //Max query try reached. Return whatever is found.
                {
                    break;
                }

                benchmarkCriteria = _benchmarkCriteriaBuilderService.BuildFromSimpleComparisonCriteria(defaultSchoolFinancialDataModel, simpleCriteria, tryCount);

                benchmarkSchools = await _financialDataService.SearchSchoolsByCriteriaAsync(benchmarkCriteria, estType);

                if (benchmarkSchools.Count > basketSize) //Number jumping to more than ideal. Cut from top by proximity.
                {
                    benchmarkSchools = benchmarkSchools.OrderBy(b => Math.Abs(b.GetPropertyValue<int>("No Pupils") - defaultSchoolFinancialDataModel.PupilCount)).Take(basketSize).ToList();
                    benchmarkCriteria.MinNoPupil = benchmarkSchools.Min(s => s.GetPropertyValue<int>("No Pupils"));
                    benchmarkCriteria.MaxNoPupil = benchmarkSchools.Max(s => s.GetPropertyValue<int>("No Pupils")); //Update the criteria to reflect the max and min pupil count of the found schools
                    break;
                }
            }

            tryCount = 1;
            while (benchmarkSchools.Count < basketSize) //Query return is still less than required
            {
                var urbanRuralDefault = defaultSchoolFinancialDataModel.UrbanRural;
                var urbanRuralKey = Dictionaries.UrbanRuralDictionary.First(d => d.Value == urbanRuralDefault).Key;

                var urbanRuralQuery = Dictionaries.UrbanRuralDictionary.Where(d =>
                    d.Key >= urbanRuralKey - tryCount && d.Key <= urbanRuralKey + tryCount).Select(d => d.Value).ToArray();

                benchmarkCriteria.UrbanRural = urbanRuralQuery;

                benchmarkSchools = await _financialDataService.SearchSchoolsByCriteriaAsync(benchmarkCriteria, estType);

                if (benchmarkSchools.Count > basketSize) //Number jumping to more than ideal. Cut from top by proximity.
                {
                    benchmarkSchools = benchmarkSchools.OrderBy(b => Math.Abs(b.GetPropertyValue<int>("No Pupils") - defaultSchoolFinancialDataModel.PupilCount)).Take(basketSize).ToList();
                    benchmarkCriteria.MinNoPupil = benchmarkSchools.Min(s => s.GetPropertyValue<int>("No Pupils"));
                    benchmarkCriteria.MaxNoPupil = benchmarkSchools.Max(s => s.GetPropertyValue<int>("No Pupils")); //Update the criteria to reflect the max and min pupil count of the found schools
                    break;
                }

                if (urbanRuralQuery.Length == Dictionaries.UrbanRuralDictionary.Count)
                {
                    break;
                }

                tryCount++;
            }

            return new ComparisonResult()
            {
                BenchmarkSchools = benchmarkSchools,
                BenchmarkCriteria = benchmarkCriteria
            };
        }
    }
}