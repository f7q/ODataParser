﻿using Sprache;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ODataParser.Test
{
    public class SinglePropertyComparisonTest
    {
        public class Data
        {
            public int Integer { get; set; }
            public string String { get; set; }
            public bool Boolean { get; set; }
        }

        [Fact]
        public void Filter_eq_Integer()
        {
            // ARRANGE
            var data = new List<Data> {
                new Data{ Integer = 2 }
            }.AsQueryable();

            // ACT
            var result = data.Where(new ComparisionExpression<Data>().MakeWhere("Integer eq 2"));

            // ASSERT
            Assert.Same(data.ElementAt(0), result.Single());
        }

        [Fact]
        public void Filter_eq_String()
        {
            // ARRANGE
            var data = new List<Data> {
                new Data{ String = "test" }
            }.AsQueryable();

            // ACT
            var result = data.Where(new ComparisionExpression<Data>().MakeWhere("String eq 'test'"));

            // ASSERT
            Assert.Same(data.ElementAt(0), result.Single());
        }

        [Fact]
        public void Filter_eq_Bool()
        {
            // ARRANGE
            var data = new List<Data> {
                new Data{ Boolean = true }
            }.AsQueryable();

            // ACT
            var result = data.Where(new ComparisionExpression<Data>().MakeWhere("Boolean eq true"));

            // ASSERT
            Assert.Same(data.ElementAt(0), result.Single());
        }
    }
}