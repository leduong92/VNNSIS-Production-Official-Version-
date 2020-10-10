using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace VNNSIS.Core 
{
    public enum commit
    {
        PGCMT_START,
        PGCMT_COMMIT,
        PGCMT_ROLLBACK,
        SQL_START,
        SQL_COMMIT,
        SQL_ROLLBACK,
        DISPOSE
    }
    public static class DataProvider
    {
        public static SqlTransaction _sqlTrans { get; private set; }
        private static SqlConnection _sqlconnection;
        private static DbConnection _connection;
        private static DbTransaction _trns;
        public static DataTable ExcuteQuery(this DbContext context, string query, Dictionary<string, object> parameters = null, CommandType commandtype = CommandType.Text)
        {
            DataTable dt = new DataTable();
            _connection = context.Database.GetDbConnection();

            if (_connection.State.Equals(ConnectionState.Closed)) { 
                    _connection.Open(); 
                   // _trns = _connection.BeginTransaction();
            }
            using (DbCommand cmd = _connection.CreateCommand())
            {
                try 
                {
                    cmd.CommandText = query;
                    cmd.Transaction = _trns;
                    if (parameters != null)
                    {
                        foreach (var parameter in parameters)
                        {
                            DbParameter dbParam = cmd.CreateParameter();
                            dbParam.ParameterName = parameter.Key;
                            dbParam.Value = parameter.Value;
                            cmd.Parameters.Add(dbParam);
                        }
                    }
                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                    return dt;
                } 
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    return new DataTable();
                } 
            }
        }
        public static int ExcuteNonQuery(this DbContext context, string query, Dictionary<string, object> parameters = null, CommandType commandtype = CommandType.Text)
        {
            _connection = context.Database.GetDbConnection();

            if (_connection.State.Equals(ConnectionState.Closed)) 
            { 
                _connection.Open();
               // _trns = _connection.BeginTransaction();
            }

            using (DbCommand cmd = _connection.CreateCommand())
            {
                try 
                {
                    cmd.CommandText = query;
                    cmd.CommandType = commandtype;
                    cmd.CommandTimeout = 3000;
                    cmd.Transaction = _trns;
                    if (parameters != null)
                    {
                        foreach (var parameter in parameters)
                        {
                            DbParameter dbParam = cmd.CreateParameter();
                            dbParam.ParameterName = parameter.Key;
                            dbParam.Value = parameter.Value;
                            cmd.Parameters.Add(dbParam);
                        }
                    }
                    return cmd.ExecuteNonQuery();
                } 
                catch (Exception e)
                {
                    _trns.Dispose();
                    Debug.WriteLine(e);
                    return -1;
                } 
            }
        }
        public static int SqlExcuteNonQuery(this DbContext context, string query, Dictionary<string, object> parameters = null, CommandType commandtype = CommandType.Text)
        {
            _sqlconnection = (SqlConnection)context.Database.GetDbConnection();

            if (_sqlconnection.State.Equals(ConnectionState.Closed)) { 
                _sqlconnection.Open(); 
                _sqlTrans = (SqlTransaction)_sqlconnection.BeginTransaction();
            }
            using (SqlCommand cmd = _sqlconnection.CreateCommand())
            {
                try 
                {
                    cmd.CommandText = query;
                    cmd.CommandType = commandtype;
                    cmd.CommandTimeout = 3000;
                    cmd.Transaction = _sqlTrans;
                    if (parameters != null)
                    {
                        foreach (var parameter in parameters)
                        {
                            DbParameter dbParam = cmd.CreateParameter();
                            dbParam.ParameterName = parameter.Key;
                            dbParam.Value = parameter.Value;
                            cmd.Parameters.Add(dbParam);
                        }
                    }
                    return cmd.ExecuteNonQuery();
                } 
                catch (Exception e)
                {
                    _sqlTrans.Dispose();
                    Debug.WriteLine(e);
                    return -1;
                } 
            }
        }
        public static void Commit(commit mode)
        {
            switch (mode)
            {
                case commit.PGCMT_START:
                    try
                    {
                        if (_connection != null)
                            _trns = _connection.BeginTransaction();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        _trns.Dispose();

                        _trns = _connection.BeginTransaction();
                    }
                    break;
                case commit.PGCMT_ROLLBACK:
                    try
                    {
                        _trns.Rollback();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        _trns.Dispose();
                    }
                    finally
                    {
                        if (_trns != null)
                            _trns.Dispose();

                        _trns = _connection.BeginTransaction();
                    }
                    break;
                case commit.PGCMT_COMMIT:
                    try
                    {
                        _trns.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        _trns.Dispose();
                    }
                    finally
                    {
                        if (_trns != null)
                            _trns.Dispose();

                        _trns = _connection.BeginTransaction();
                    }
                    break;
                default:
                    _trns.Dispose();
                    break;
            }
        }
        public static void SqlCommit(commit mode)
        {
            switch (mode)
            {
                case commit.SQL_ROLLBACK:
                    try
                    {
                        _sqlTrans.Rollback();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        _sqlTrans.Dispose();
                    }
                    finally
                    {
                        if (_sqlTrans != null)
                            _sqlTrans.Dispose();
                    }
                    break;
                case commit.SQL_COMMIT:
                    try
                    {
                        _sqlTrans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        _sqlTrans.Dispose();
                    }
                    finally
                    {
                        if (_sqlTrans != null)
                            _sqlTrans.Dispose();
                        
                    }
                    break;
                default:
                    _sqlTrans.Dispose();
                    break;
            }
        }
    }
}