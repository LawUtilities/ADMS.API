﻿using ADMS.API.Services;
using System.Linq.Dynamic.Core;

namespace ADMS.API.Helpers;

/// <summary>
/// Iqueryable Extension methods
/// </summary>
public static class IQueryableExtensions
{
    /// <summary>
    /// Apply Sort method
    /// </summary>
    /// <typeparam name="T">Data Type</typeparam>
    /// <param name="source">Source</param>
    /// <param name="orderBy">Order By field</param>
    /// <param name="mappingDictionary">Mappibnd Dictionary</param>
    /// <returns>IQueryable of type T</returns>
    /// <exception cref="ArgumentException">Property name missing</exception>
    /// <exception cref="ArgumentNullException">source, mappibngDictionary or propertyMappingValue null</exception>
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> source, 
        string orderBy,
        Dictionary<string, PropertyMappingValue> mappingDictionary)
    {
        ArgumentNullException.ThrowIfNull(source);

        ArgumentNullException.ThrowIfNull(mappingDictionary);

        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return source;
        }

        var orderByString = string.Empty;

        // the orderBy string is separated by ",", so we split it.
        var orderByAfterSplit = orderBy.Split(',');

        // apply each orderby clause  
        foreach (var orderByClause in orderByAfterSplit)
        {
            // trim the orderBy clause, as it might contain leading
            // or trailing spaces. Can't trim the var in foreach,
            // so use another var.
            var trimmedOrderByClause = orderByClause.Trim();

            // if the sort option ends with with " desc", we order
            // descending, ortherwise ascending
            var orderDescending = trimmedOrderByClause.EndsWith(" desc");

            // remove " asc" or " desc" from the orderBy clause, so we 
            // get the property name to look for in the mapping dictionary
            int indexOfFirstSpace = trimmedOrderByClause.IndexOf(' ');
            var propertyName = indexOfFirstSpace == -1 ?
                trimmedOrderByClause : trimmedOrderByClause
                .Remove(indexOfFirstSpace);

            // find the matching property
            if (!mappingDictionary.TryGetValue(propertyName, out PropertyMappingValue? value))
            {
                throw new ArgumentException($"Key mapping for {propertyName} is missing");
            }

            // get the PropertyMappingValue
            var propertyMappingValue = value;

            if (propertyMappingValue != null)
            {
                // revert sort order if necessary
                if (propertyMappingValue.Revert)
                {
                    orderDescending = !orderDescending;
                }

                // Run through the property names 
                foreach (var destinationProperty in
                    propertyMappingValue.DestinationProperties)
                {
                    orderByString = orderByString +
                        (string.IsNullOrWhiteSpace(orderByString) ? string.Empty : ", ")
                        + destinationProperty
                        + (orderDescending ? " descending" : " ascending");
                }
            }
            else
            {
                throw new ArgumentException(nameof(propertyMappingValue));
            }
        }

        return source.OrderBy(orderByString);
    }
}

