﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using SFB.Web.Common;

namespace SFB.Web.DAL.Repositories
{
    public interface IFinancialDataRepository
    {
        Document GetSchoolDataDocument(string urn, string term, SchoolFinancialType schoolFinancialType, CentralFinancingType cFinance = CentralFinancingType.Exclude);
        dynamic GetAcademiesByMatNumber(string term, string matNo);
        Document GetMATDataDocument(string matNo, string term, MatFinancingType matFinance);
        Task<List<Document>> SearchSchoolsByCriteriaAsync(BenchmarkCriteria criteria, EstablishmentType estType);
        Task<int> SearchSchoolsCountByCriteriaAsync(BenchmarkCriteria criteria, EstablishmentType estType);
    }
}