﻿using Dapper;
using System;
using System.Collections.Generic;

namespace MainMusicStore.DataAccess.IMainRepository
{
    public interface ISPCallRepository:IDisposable
    {
        T Single<T>(String procedureName, DynamicParameters parameters = null);
        void Execute(String procedureName, DynamicParameters parameters = null);
        T OneRecord<T>(String procedureName, DynamicParameters parameters = null);
        IEnumerable<T> List<T>(String procedureName, DynamicParameters parameters = null);
        Tuple<IEnumerable<T1>, IEnumerable<T2>> List<T1, T2>(String procedureName, DynamicParameters parameters = null);
    }
}
