﻿using NhapHangV2.Entities.DomainEntities;
using NhapHangV2.Interface;
using NhapHangV2.Interface.UnitOfWork;
using NhapHangV2.Utilities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NhapHangV2.Interface.Services.DomainServices;

namespace NhapHangV2.Service.Services.DomainServices
{
    public abstract class DomainService<E, T> : IDomainService<E, T> where E : Entities.DomainEntities.AppDomain where T : BaseSearch, new()
    {
        protected readonly IUnitOfWork unitOfWork;
        protected readonly IMapper mapper;
        protected IQueryable<E> Queryable
        {
            get
            {
                return unitOfWork.Repository<E>().GetQueryable().AsNoTracking();
            }
        }
        public DomainService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public virtual void LoadReferences(IList<E> items)
        {
        }

        public virtual bool Save(E item)
        {
            return Save(new List<E> { item });
        }
        public virtual bool Save(IList<E> items)
        {
            foreach (var item in items)
            {
                var exists = Queryable
                .AsNoTracking()
                .Where(e =>
                e.Id == item.Id
                && !e.Deleted)
                .FirstOrDefault();
                if (exists != null)
                {
                    if (item.Deleted)
                    {
                        Delete(item.Id);
                    }
                    else
                    {
                        exists = mapper.Map<E>(item);
                        unitOfWork.Repository<E>().SetEntityState(exists, EntityState.Modified);
                    }
                }
                else
                {
                    unitOfWork.Repository<E>().Create(item);
                }
            }
            unitOfWork.Save();
            return true;
        }

        public virtual bool IsSafeDelete(int id)
        {
            //try
            //{
            //    string query = string.Format(@"BEGIN TRAN DELETE FROM {0} WHERE ID = @Id; ROLLBACK TRAN", typeof(E).Name);
            //    unitOfWork.Repository<E>().ExecuteNonQuery(query, new SqlParameter[]{
            //        new SqlParameter()
            //        {
            //            ParameterName = "@Id",
            //            Value = id
            //        }
            //    });
            //}
            //catch (Exception ex)
            //{
            //    return false;
            //}

            return true;
        }

        public virtual bool Delete(int id)
        {
            var exists = Queryable
                .Where(e => e.Id == id)
                .FirstOrDefault();
            if (exists != null)
            {
                exists.Deleted = true;
                unitOfWork.Repository<E>().Update(exists);
                unitOfWork.Save();
                return true;
            }
            else
            {
                throw new Exception(id + " not exists");
            }
        }


        public virtual IList<E> GetAll()
        {
            return GetAll(null);
        }
        public virtual IList<E> GetAll(Expression<Func<E, E>> select)
        {
            var query = Queryable.Where(e => !e.Deleted);
            if (select != null)
            {
                query = query.Select(select);
            }
            return query.ToList();
        }

        public virtual async Task<PagedList<E>> GetPagedListData(T baseSearch)
        {
            PagedList<E> pagedList = new PagedList<E>();
            SqlParameter[] parameters = GetSqlParameters(baseSearch);
            pagedList = await this.unitOfWork.Repository<E>().ExcuteQueryPagingAsync(this.GetStoreProcName(), parameters);
            pagedList.PageIndex = baseSearch.PageIndex;
            pagedList.PageSize = baseSearch.PageSize;
            return pagedList;
        }

        public virtual async Task<E> GetDataById(int id)
        {
            SqlParameter parameter = new SqlParameter("@Id", id);
            E data = await this.unitOfWork.Repository<E>().ExcuteQueryGetByIdAsync(this.GetStoreProcNameGetById(), parameter);
            return data;
        }

        /// <summary>
        /// Lấy thông tin tên procedure cần exec
        /// </summary>
        /// <returns></returns>
        protected virtual string GetStoreProcName()
        {
            return string.Empty;
        }

        /// <summary>
        /// Lấy thông tin tên procedure cần exec
        /// </summary>
        /// <returns></returns>
        protected virtual string GetStoreProcNameGetById()
        {
            return string.Empty;
        }

        protected virtual SqlParameter[] GetSqlParameters(T baseSearch)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            foreach (PropertyInfo prop in baseSearch.GetType().GetProperties())
            {
                sqlParameters.Add(new SqlParameter(prop.Name, prop.GetValue(baseSearch, null)));
            }
            SqlParameter[] parameters = sqlParameters.ToArray();
            return parameters;
        }


        public virtual E GetById(int id)
        {
            return GetById(id, (IConfigurationProvider)null);
        }
        public virtual E GetById(int id, Expression<Func<E, E>> select)
        {
            var query = Queryable.Where(e => !e.Deleted);
            if (select != null)
            {
                query = query.Select(select);
            }
            return query
                 .AsNoTracking()
                 .Where(e => e.Id == id)
                 .FirstOrDefault();
        }

        public IList<E> Get(Expression<Func<E, bool>> expression)
        {
            return Get(new Expression<Func<E, bool>>[] { expression });
        }

        public IList<E> Get(Expression<Func<E, bool>> expression, Expression<Func<E, E>> select)
        {
            return Get(new Expression<Func<E, bool>>[] { expression }, select);
        }
        public IList<E> Get(Expression<Func<E, bool>> expression, IConfigurationProvider mapperConfiguration)
        {
            if (mapperConfiguration == null)
            {
                return Get(expression);
            }
            else
            {
                return Get(new Expression<Func<E, bool>>[] { expression }, mapperConfiguration);
            }
        }
        public virtual async Task<bool> SaveAsync(E item)
        {
            return await SaveAsync(new List<E> { item });
        }

        public virtual async Task<bool> CreateAsync(E item)
        {
            return await CreateAsync(new List<E> { item });
        }

        public virtual async Task<bool> CreateAsync(IList<E> items)
        {
            foreach (var item in items)
            {
                await unitOfWork.Repository<E>().CreateAsync(item);
            }
            await unitOfWork.SaveAsync();
            return true;
        }
        public virtual async Task<bool> UpdateAsync(E item)
        {
            return await UpdateAsync(new List<E> { item });
        }

        public virtual async Task<bool> UpdateAsync(IList<E> items)
        {
            foreach (var item in items)
            {
                var exists = await Queryable
                 .AsNoTracking()
                 .Where(e => e.Id == item.Id && !e.Deleted)
                 .FirstOrDefaultAsync();

                if (exists != null)
                {
                    var currentCreated = exists.Created;
                    var currentCreatedByInfo = exists.CreatedBy;
                    exists = mapper.Map<E>(item);
                    exists.Created = currentCreated;
                    exists.CreatedBy = currentCreatedByInfo;
                    unitOfWork.Repository<E>().Update(exists);
                }
            }
            await unitOfWork.SaveAsync();
            return true;
        }

        /// <summary>
        /// Cập nhật theo field
        /// </summary>
        /// <param name="item"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public async Task<bool> UpdateFieldAsync(E item, params Expression<Func<E, object>>[] includeProperties)
        {
            return await UpdateFieldAsync(new List<E> { item }, includeProperties);
        }

        /// <summary>
        /// Cập nhật theo field
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public async Task<bool> UpdateFieldAsync(IList<E> items, params Expression<Func<E, object>>[] includeProperties)
        {
            foreach (var item in items)
            {
                var exists = await Queryable
                 .AsNoTracking()
                 .Where(e => e.Id == item.Id && !e.Deleted)
                 .FirstOrDefaultAsync();

                if (exists != null)
                {
                    exists = mapper.Map<E>(item);
                    unitOfWork.Repository<E>().UpdateFieldsSave(exists, includeProperties);
                }
            }
            await unitOfWork.SaveAsync();
            return true;
        }



        public async Task<bool> SaveAsync(IList<E> items)
        {
            foreach (var item in items)
            {
                var exists = await Queryable
                 .AsNoTracking()
                 .Where(e => e.Id == item.Id && !e.Deleted)
                 .FirstOrDefaultAsync();

                if (exists != null)
                {
                    exists = mapper.Map<E>(item);
                    unitOfWork.Repository<E>().Update(exists);
                }
                else
                {
                    await unitOfWork.Repository<E>().CreateAsync(item);
                }
            }
            await unitOfWork.SaveAsync();
            return true;
        }

        public virtual async Task<bool> DeleteAsync(int id)
        {
            var exists = Queryable
                .AsNoTracking()
                .FirstOrDefault(e => e.Id == id);
            if (exists != null)
            {
                exists.Deleted = true;
                unitOfWork.Repository<E>().Update(exists);
                await unitOfWork.SaveAsync();
                return true;
            }
            else
            {
                throw new Exception(id + " not exists");
            }
        }

        public async Task<IList<E>> GetAllAsync()
        {
            return await Queryable.AsNoTracking().ToListAsync();
        }

        public virtual async Task<E> GetByIdAsync(int id)
        {
            return await Queryable.Where(e => e.Id == id && !e.Deleted).AsNoTracking().FirstOrDefaultAsync();
        }

        public virtual async Task<E> GetByIdAsync(int id, Expression<Func<E, E>> select)
        {
            var query = unitOfWork.Repository<E>()
               .GetQueryable()
               .Where(e => !e.Deleted)
               .AsNoTracking();
            if (select != null)
            {
                query = query.Select(select);
            }
            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }

        public virtual async Task<IList<E>> GetAllAsync(Expression<Func<E, E>> select)
        {
            return await Queryable
                .Select(select)
                .ToListAsync();

        }

        public async Task<IList<E>> GetAsync(Expression<Func<E, bool>> expression, Expression<Func<E, E>> select)
        {
            return await unitOfWork.Repository<E>()
                 .GetQueryable()
                 .Where(expression)
                 .Select(select)
                 .ToListAsync();
        }

        public async Task<IList<E>> GetAsync(Expression<Func<E, bool>> expression)
        {
            return await unitOfWork.Repository<E>()
                .GetQueryable()
                .Where(expression)
                .ToListAsync();
        }




        public async Task<IList<E>> GetAsync(Expression<Func<E, bool>> expression, bool useProjectTo)
        {
            if (useProjectTo)
                return await unitOfWork.Repository<E>()
                .GetQueryable()
                .ProjectTo<E>(mapper.ConfigurationProvider)
                .Where(expression)
                .ToListAsync();
            return await unitOfWork.Repository<E>()
                .GetQueryable()
                .ProjectTo<E>(mapper.ConfigurationProvider)
                .Where(expression)
                .ToListAsync();
        }

        public async Task<E> GetSingleAsync(Expression<Func<E, bool>> expression, Expression<Func<E, E>> select)
        {
            return await unitOfWork.Repository<E>()
                 .GetQueryable()
                 .Where(expression)
                 .Select(select)
                 .FirstOrDefaultAsync();
        }

        public async Task<E> GetSingleAsync(Expression<Func<E, bool>> expression)
        {
            return await unitOfWork.Repository<E>()
                .GetQueryable()
                .Where(expression)
                .FirstOrDefaultAsync();
        }

        public async Task<E> GetSingleAsync(Expression<Func<E, bool>> expression, bool useProjectTo)
        {
            if (useProjectTo)
                return await unitOfWork.Repository<E>()
                .GetQueryable()
                .ProjectTo<E>(mapper.ConfigurationProvider)
                .Where(expression)
                .FirstOrDefaultAsync();
            return await unitOfWork.Repository<E>()
                .GetQueryable()
                .ProjectTo<E>(mapper.ConfigurationProvider)
                .Where(expression)
                .FirstOrDefaultAsync();
        }

        public E GetById(int id, IConfigurationProvider mapperConfiguration)
        {
            var queryable = Queryable.Where(e => !e.Deleted && e.Id == id);
            if (mapperConfiguration != null)
                queryable = queryable.ProjectTo<E>(mapperConfiguration);
            return queryable.AsNoTracking().FirstOrDefault();
        }

        public virtual async Task<E> GetByIdAsync(int id, IConfigurationProvider mapperConfiguration)
        {
            var queryable = Queryable.Where(e => !e.Deleted && e.Id == id);
            if (mapperConfiguration != null)
                queryable = queryable.ProjectTo<E>(mapperConfiguration);
            return await queryable.AsNoTracking().FirstOrDefaultAsync();
        }

        public IList<E> Get(Expression<Func<E, bool>>[] expressions, Expression<Func<E, E>> select)
        {
            var queryable = Queryable.Where(e => !e.Deleted);
            foreach (var expression in expressions)
            {
                queryable = queryable.Where(expression);
            }
            if (select != null)
                queryable = queryable.Select(select);
            return queryable.ToList();
        }

        public IList<E> Get(Expression<Func<E, bool>>[] expressions)
        {
            var queryable = Queryable.Where(e => !e.Deleted);
            foreach (var expression in expressions)
            {
                queryable = queryable.Where(expression);
            }
            return queryable.ToList();
        }

        public IList<E> Get(Expression<Func<E, bool>>[] expressions, IConfigurationProvider mapperConfiguration)
        {
            var queryable = Queryable.Where(e => !e.Deleted);
            foreach (var expression in expressions)
            {
                queryable = queryable.Where(expression);
            }
            queryable = queryable
                .ProjectTo<E>(mapperConfiguration);
            return queryable.ToList();
        }

        public async Task<IList<E>> GetAsync(Expression<Func<E, bool>>[] expressions, Expression<Func<E, E>> select)
        {
            var queryable = Queryable.Where(e => !e.Deleted);
            foreach (var expression in expressions)
            {
                queryable = queryable.Where(expression);
            }
            if (select != null)
            {
                queryable = queryable.Select(select);
            }
            return await queryable.ToListAsync();
        }

        public async Task<IList<E>> GetAsync(Expression<Func<E, bool>>[] expressions)
        {
            var queryable = Queryable.Where(e => !e.Deleted);
            foreach (var expression in expressions)
            {
                queryable = queryable.Where(expression);
            }

            return await queryable.ToListAsync();
        }

        public async Task<IList<E>> GetAsync(Expression<Func<E, bool>>[] expressions, bool useProjectTo)
        {
            var queryable = Queryable.Where(e => !e.Deleted);
            foreach (var expression in expressions)
            {
                queryable = queryable.Where(expression);
            }
            if (useProjectTo)
            {
                queryable = queryable.ProjectTo<E>(mapper.ConfigurationProvider);
            }
            return await queryable.ToListAsync();
        }

        public async Task<E> GetSingleAsync(Expression<Func<E, bool>>[] expressions, Expression<Func<E, E>> select)
        {
            var queryable = Queryable.Where(e => !e.Deleted);
            foreach (var expression in expressions)
            {
                queryable = queryable.Where(expression);
            }
            if (select != null)
            {
                queryable = queryable.Select(select);
            }
            return await queryable.FirstOrDefaultAsync();
        }

        public async Task<E> GetSingleAsync(Expression<Func<E, bool>>[] expressions)
        {
            var queryable = Queryable.Where(e => !e.Deleted);
            foreach (var expression in expressions)
            {
                queryable = queryable.Where(expression);
            }

            return await queryable.FirstOrDefaultAsync();
        }

        public async Task<E> GetSingleAsync(Expression<Func<E, bool>>[] expressions, bool useProjectTo)
        {
            var queryable = Queryable.Where(e => !e.Deleted);
            foreach (var expression in expressions)
            {
                queryable = queryable.Where(expression);
            }
            if (useProjectTo)
            {
                queryable = queryable.ProjectTo<E>(mapper.ConfigurationProvider);
            }
            return await queryable.FirstOrDefaultAsync();
        }

        public virtual async Task<string> GetExistItemMessage(E item)
        {
            return await Task.Run(() =>
            {
                List<string> messages = new List<string>();
                string result = string.Empty;
                if (messages.Any())
                    result = string.Join(" ", messages);
                return result;
            });
        }

    }
}
