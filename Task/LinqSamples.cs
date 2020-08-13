// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using SampleSupport;
using Task.Data;

// Version Mad01

namespace SampleQueries
{
    [Title("LINQ Module")]
    [Prefix("Linq")]
    public class LinqSamples : SampleHarness
    {

        private DataSource dataSource = new DataSource();

        [Category("Main category")]
        [Title("Task 1")]
        [Description("Customers with total orders greater than x")]
        public void Linq1()
        {
            var x = 3000;

            var customers = dataSource.Customers
                .Where(c => c.Orders.Sum(o => o.Total) > x)
                .Select(c => new
                {
                    CustomerId = c.CustomerID,
                    TotalSum = c.Orders.Sum(o => o.Total)
                });

            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine("_____________________________________");
                Console.WriteLine($"More than: {x}");

                foreach (var customer in customers)
                {
                    ObjectDumper.Write(
                        string.Format($"CustomerId = {customer.CustomerId} TotalSum = {customer.TotalSum}\n"));
                }

                x *= 5;
            }
        }

        [Category("Main category")]
        [Title("Task 2")]
        [Description("Customers with same country and city")]

        public void Linq2()
        {
            var resultWithoutGroup = dataSource.Customers
                .Select(c => new
                {
                    Customer = c,
                    Suppliers = dataSource.Suppliers
                    .Where(s =>
                    s.City == c.City && s.Country == c.Country)
                });

            var resultWithGroup = dataSource.Customers.GroupJoin(dataSource.Suppliers,
                c => new { c.City, c.Country },
                s => new { s.City, s.Country },
                (c, s) => new { Customer = c, Suppliers = s });

            Console.WriteLine("__________________________________________________________");
            Console.WriteLine("With  group:");
            foreach (var customer in resultWithGroup)
            {
                var supliersName = customer.Suppliers.Select(s => s.SupplierName);

                ObjectDumper.Write($"CustomerId: {customer.Customer.CustomerID} " +
                     $"List of suppliers: {string.Join(", ", supliersName)}");
            }

            Console.WriteLine("__________________________________________________________");
            Console.WriteLine("Without group");
            foreach (var customer in resultWithoutGroup)
            {
                var supliersName = customer.Suppliers.Select(s => s.SupplierName);

                ObjectDumper.Write($"CustomerId: {customer.Customer.CustomerID} " +
                     $"List of suppliers: {string.Join(", ", supliersName)}");
            }
        }

        [Category("Main category")]
        [Title("Task 3")]
        [Description("Customers with total orders greater than X")]

        public void Linq3()
        {
            var x = 10000;
            var customers = dataSource.Customers.Where(c => c.Orders.Any(s => s.Total > x));

            Console.WriteLine("Customers: ");
            foreach (var customer in customers)
            {
                ObjectDumper.Write(customer);
            }
        }

        [Category("Main category")]
        [Title("Task 4")]
        [Description("Customers by year and month")]

        public void Linq4()
        {
            var customers = dataSource.Customers.Where(c => c.Orders.Any())
                .Select(c => new
                {
                    CustomerId = c.CustomerID,
                    StartDate = c.Orders.OrderBy(o => o.OrderDate).Select(o => o.OrderDate).First()
                });

            foreach (var c in customers)
            {
                ObjectDumper.Write($"CustomerId: {c.CustomerId} Month: {c.StartDate.Month} Year: {c.StartDate.Year}");
            }
        }

        [Category("Main category")]
        [Title("Task 5")]
        [Description("Customers ordered by year, month, total orders, clientName")]

        public void Linq5()
        {
            var customers = dataSource.Customers.Where(c => c.Orders.Any())
                .Select(c => new
                {
                    CustomerId = c.CustomerID,
                    StartDate = c.Orders.OrderBy(o => o.OrderDate).Select(o => o.OrderDate).First(),
                    TotalSum = c.Orders.Sum(o => o.Total)
                }).OrderByDescending(c => c.StartDate.Year)
                .ThenByDescending(c => c.StartDate.Month)
                .ThenByDescending(c => c.TotalSum)
                .ThenByDescending(c => c.CustomerId);

            foreach (var customer in customers)
            {
                ObjectDumper.Write($"CustomerId: {customer.CustomerId} TotalSum: {customer.TotalSum} Month:" +
                    $"{customer.StartDate.Month} Year: {customer.StartDate.Year}");
            }
        }

        [Category("Main category")]
        [Title("Task 6")]
        [Description("Customers without region or whithout operators code")]
        public void Linq6()
        {
            var customers = dataSource.Customers.Where(
                c => c.PostalCode != null
                    && c.PostalCode.Any(x => x < '0' || x > '9')
                    || string.IsNullOrWhiteSpace(c.Region)
                    || c.Phone.FirstOrDefault() != '(');

            foreach (var c in customers)
            {
                ObjectDumper.Write(c);
            }
        }

        [Category("Main category")]
        [Title("Task 7")]
        [Description("Products group by categories")]
        public void Linq7()
        {
            var products = dataSource.Products.GroupBy(p => p.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    ProductsByStock = g.GroupBy(p => p.UnitsInStock > 0)
                        .Select(a => new
                        {
                            HasInStock = a.Key,
                            Products = a.OrderBy(prod => prod.UnitPrice)
                        })
                });

            foreach (var p in products)
            {
                ObjectDumper.Write($"Category: {p.Category}\n");

                foreach (var productsByStock in p.ProductsByStock)
                {
                    ObjectDumper.Write($"\tHas in stock: {productsByStock.HasInStock}");

                    foreach (var product in productsByStock.Products)
                    {
                        ObjectDumper.Write($"\t\tProduct: {product.ProductName} Price: {product.UnitPrice}");
                    }
                }
            }
        }

        [Category("Main category")]
        [Title("Task 8")]
        [Description("Products group by price: cheap, average price, expensive")]
        public void Linq8()
        {
            var expensiveAverageBoundary = 50;
            var lowAverageBoundary = 20;

            var productGroups = dataSource.Products
                .GroupBy(p => p.UnitPrice < lowAverageBoundary ? "Cheap"
                    : p.UnitPrice < expensiveAverageBoundary ? "Average price" : "Expensive");

            foreach (var group in productGroups)
            {
                ObjectDumper.Write($"{group.Key}:");

                foreach (var product in group)
                {
                    ObjectDumper.Write($"\tProduct: {product.ProductName} Price: {product.UnitPrice}\n");
                }
            }
        }

        [Category("Main category")]
        [Title("Task 9")]
        [Description("Average total order for each city")]
        public void Linq9()
        {
            var resAvg = dataSource.Customers
                .GroupBy(c => c.City)
                .Select(c => new
                {
                    City = c.Key,
                    Intensity = c.Average(p => p.Orders.Length),
                    AverageIncome = c.Average(p => p.Orders.Sum(o => o.Total))
                });

            foreach (var group in resAvg)
            {
                ObjectDumper.Write($"City: {group.City}");
                ObjectDumper.Write($"\tIntensity: {group.Intensity}");
                ObjectDumper.Write($"\tAverage Income: {group.AverageIncome}");
            }
        }

        [Category("Main category")]
        [Title("Task 10")]
        [Description("Average clients activity statistic by month")]
        public void Linq10()
        {
            var statistic = dataSource.Customers
                .Select(c => new
                {
                    c.CustomerID,
                    MonthsStatistic = c.Orders.GroupBy(o => o.OrderDate.Month)
                                        .Select(g => new { Month = g.Key, OrdersCount = g.Count() }),
                    YearsStatistic = c.Orders.GroupBy(o => o.OrderDate.Year)
                                        .Select(g => new { Year = g.Key, OrdersCount = g.Count() }),
                    YearMonthStatistic = c.Orders
                                        .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                                        .Select(g => new { g.Key.Year, g.Key.Month, OrdersCount = g.Count() })
                });

            foreach (var record in statistic)
            {
                ObjectDumper.Write($"CustomerId: {record.CustomerID}");
                Console.WriteLine("\tMonths statistic:\n");

                foreach (var ms in record.MonthsStatistic)
                {
                    ObjectDumper.Write($"\t\tMonth: {ms.Month} Orders count: {ms.OrdersCount}");
                }

                ObjectDumper.Write("\tYears statistic:\n");

                foreach (var ys in record.YearsStatistic)
                {
                    ObjectDumper.Write($"\t\tYear: {ys.Year} Orders count: {ys.OrdersCount}");
                }

                ObjectDumper.Write("\tYear and month statistic:\n");

                foreach (var yms in record.YearMonthStatistic)
                {
                    ObjectDumper.Write($"\t\tYear: {yms.Year} Month: {yms.Month} Orders count: {yms.OrdersCount}");
                }
            }
        }
    }
}
