using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NhapHangV2.Entities;
using NhapHangV2.Interface.DbContext;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.UnitOfWork;
using NhapHangV2.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Service.Services
{
    public class DashboardService : IDashboardService
    {
        protected readonly IAppDbContext Context;
        protected readonly IAppUnitOfWork unitOfWork;
        protected readonly IMapper mapper;

        public DashboardService(IAppUnitOfWork unitOfWork, IAppDbContext Context, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.Context = Context;
            this.mapper = mapper;
        }

        public async Task<List<Dashboard_GetTotalInWeek>> GetTotalInWeek()
        {
            return await Task.Run(() =>
            {
                List<Dashboard_GetTotalInWeek> dashBoards = new List<Dashboard_GetTotalInWeek>();
                DataTable dataTable = new DataTable();
                SqlConnection connection = null;
                SqlCommand command = null;
                try
                {
                    connection = (SqlConnection)Context.Database.GetDbConnection();
                    command = connection.CreateCommand();
                    connection.Open();
                    command.CommandText = "Dashboard_GetTotalInWeek";
                    command.CommandType = CommandType.StoredProcedure;
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                    sqlDataAdapter.Fill(dataTable);
                    dashBoards = MappingDataTable.ConvertToList<Dashboard_GetTotalInWeek>(dataTable);
                    return dashBoards;
                }
                finally
                {
                    if (connection != null && connection.State == System.Data.ConnectionState.Open)
                        connection.Close();

                    if (command != null)
                        command.Dispose();
                }
            });
        }

        public async Task<List<Dashboard_GetItemInWeek>> GetItemInWeek()
        {
            return await Task.Run(() =>
            {
                List<Dashboard_GetItemInWeek> dashBoards = new List<Dashboard_GetItemInWeek>();
                DataTable dataTable = new DataTable();
                SqlConnection connection = null;
                SqlCommand command = null;
                try
                {
                    connection = (SqlConnection)Context.Database.GetDbConnection();
                    command = connection.CreateCommand();
                    connection.Open();
                    command.CommandText = "Dashboard_GetItemInWeek";
                    command.CommandType = CommandType.StoredProcedure;
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                    sqlDataAdapter.Fill(dataTable);
                    dashBoards = MappingDataTable.ConvertToList<Dashboard_GetItemInWeek>(dataTable);
                    return dashBoards;
                }
                finally
                {
                    if (connection != null && connection.State == System.Data.ConnectionState.Open)
                        connection.Close();

                    if (command != null)
                        command.Dispose();
                }
            });
        }


        public async Task<List<Dashboard_GetPercentOrder>> GetPercentOrder()
        {
            return await Task.Run(() =>
            {
                List<Dashboard_GetPercentOrder> dashBoards = new List<Dashboard_GetPercentOrder>();
                DataTable dataTable = new DataTable();
                SqlConnection connection = null;
                SqlCommand command = null;
                try
                {
                    connection = (SqlConnection)Context.Database.GetDbConnection();
                    command = connection.CreateCommand();
                    connection.Open();
                    command.CommandText = "Dashboard_GetPerCentOrder";
                    command.CommandType = CommandType.StoredProcedure;
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                    sqlDataAdapter.Fill(dataTable);
                    dashBoards = MappingDataTable.ConvertToList<Dashboard_GetPercentOrder>(dataTable);
                    return dashBoards;
                }
                finally
                {
                    if (connection != null && connection.State == System.Data.ConnectionState.Open)
                        connection.Close();

                    if (command != null)
                        command.Dispose();
                }
            });
        }

        public async Task<List<Dashboard_GetPercentOrder>> GetPercentTransOrder()
        {
            return await Task.Run(() =>
            {
                List<Dashboard_GetPercentOrder> dashBoards = new List<Dashboard_GetPercentOrder>();
                DataTable dataTable = new DataTable();
                SqlConnection connection = null;
                SqlCommand command = null;
                try
                {
                    connection = (SqlConnection)Context.Database.GetDbConnection();
                    command = connection.CreateCommand();
                    connection.Open();
                    command.CommandText = "Dashboard_GetPerCentOrderTrans";
                    command.CommandType = CommandType.StoredProcedure;
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                    sqlDataAdapter.Fill(dataTable);
                    dashBoards = MappingDataTable.ConvertToList<Dashboard_GetPercentOrder>(dataTable);
                    return dashBoards;
                }
                finally
                {
                    if (connection != null && connection.State == System.Data.ConnectionState.Open)
                        connection.Close();

                    if (command != null)
                        command.Dispose();
                }
            });
        }

        public async Task<List<Dashboard_GetPercentOrder>> GetPercentPayHelp()
        {
            return await Task.Run(() =>
            {
                List<Dashboard_GetPercentOrder> dashBoards = new List<Dashboard_GetPercentOrder>();
                DataTable dataTable = new DataTable();
                SqlConnection connection = null;
                SqlCommand command = null;
                try
                {
                    connection = (SqlConnection)Context.Database.GetDbConnection();
                    command = connection.CreateCommand();
                    connection.Open();
                    command.CommandText = "Dashboard_GetPerCentOrderPayHelp";
                    command.CommandType = CommandType.StoredProcedure;
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                    sqlDataAdapter.Fill(dataTable);
                    dashBoards = MappingDataTable.ConvertToList<Dashboard_GetPercentOrder>(dataTable);
                    return dashBoards;
                }
                finally
                {
                    if (connection != null && connection.State == System.Data.ConnectionState.Open)
                        connection.Close();

                    if (command != null)
                        command.Dispose();
                }
            });
        }
    }
}
