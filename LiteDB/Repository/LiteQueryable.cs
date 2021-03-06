﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace LiteDB
{
    /// <summary>
    /// An IQueryable-like class to write fluent query in LiteDB
    /// </summary>
    public class LiteQueryable<T>
    {
        private int _limit = int.MaxValue;
        private int _skip = 0;
        private LiteCollection<T> _collection;
        private Query _query;

        internal LiteQueryable(LiteCollection<T> collection)
        {
            _collection = collection;
            _query = null;
        }

        #region Includes

        /// <summary>
        /// Include DBRef field in result query execution
        /// </summary>
        public LiteQueryable<T> Include<K>(Expression<Func<T, K>> dbref)
        {
            _collection = _collection.Include(dbref);
            return this;
        }

        /// <summary>
        /// Include DBRef path in result query execution
        /// </summary>
        public LiteQueryable<T> Include(string path)
        {
            _collection = _collection.Include(path);
            return this;
        }

        #endregion

        #region Where/Skip/Limit

        /// <summary>
        /// Add new Query filter when query will be executed. This filter use database index
        /// </summary>
        public LiteQueryable<T> Where(Query query)
        {
            _query = _query == null ? query : Query.And(_query, query);
            return this;
        }

        /// <summary>
        /// Add new Query filter when query will be executed. This filter use database index
        /// </summary>
        public LiteQueryable<T> Where(Expression<Func<T, bool>> predicate)
        {
            return this.Where(_collection.Visitor.Visit(predicate));
        }

        /// <summary>
        /// Add new Query filter when query will be executed only with "condition" is true. This filter use database index
        /// </summary>
        public LiteQueryable<T> Where(bool condition, Query query)
        {
            return condition ? this.Where(query) : this;
        }

        /// <summary>
        /// Add new Query filter when query will be executed only with "condition" is true. This filter use database index
        /// </summary>
        public LiteQueryable<T> Where(bool condition, Expression<Func<T, bool>> predicate)
        {
            return condition ? this.Where(predicate) : this;
        }

        /// <summary>
        /// Skip N results before starts returing entities
        /// </summary>
        public LiteQueryable<T> Skip(int skip)
        {
            _skip = skip;
            return this;
        }

        /// <summary>
        /// Limit (Take) results 
        /// </summary>
        public LiteQueryable<T> Limit(int limit)
        {
            _limit = limit;
            return this;
        }

        #endregion

        #region Execute Single/First

        public T Single()
        {
            return this.ToEnumerable().Single();
        }

        public T SingleOrDefault()
        {
            return this.ToEnumerable().SingleOrDefault();
        }

        public T First()
        {
            return this.ToEnumerable().First();
        }

        public T FirstOrDefault()
        {
            return this.ToEnumerable().FirstOrDefault();
        }

        /// <summary>
        /// Return entity by _id key. Throws InvalidOperationException if no document
        /// </summary>
        public T SingleById(BsonValue id)
        {
            return _collection.Find(Query.EQ("_id", id)).Single();
        }

        #endregion

        #region Execute Lists

        /// <summary>
        /// Execute query returning IEnumerable results.
        /// </summary>
        public IEnumerable<T> ToEnumerable()
        {
            return _collection.Find(_query ?? Query.All(), _skip, _limit);
        }

        /// <summary>
        /// Execute query and return results as a List
        /// </summary>
        public List<T> ToList()
        {
            return this.ToEnumerable().ToList();
        }

        /// <summary>
        /// Execute query and return results as an Array
        /// </summary>
        public T[] ToArray()
        {
            return this.ToEnumerable().ToArray();
        }

        #endregion

        #region Execute Count

        /// <summary>
        /// Execute Count methos in filter query
        /// </summary>
        public int Count()
        {
            return _query == null ? _collection.Count() : _collection.Count(_query);
        }

        /// <summary>
        /// Returns true/false if filter returns any result
        /// </summary>
        public bool Exists()
        {
            return _collection.Exists(_query ?? Query.All());
        }

        #endregion
    }
}