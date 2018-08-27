using System;
using MassTransit.Gateway.Dynamics;
using MassTransit.Gateway.Sql.Database;

namespace MassTransit.Gateway.Sql.Dynamics
{
    public static class PropertyDefinitionFactory
    {
        public static PropertyDefinition FromDbColumnInfo(DbSchemaReader.DbColumnInfo dbColumnInfo)
            => new PropertyDefinition(dbColumnInfo.Name, Type.GetType(dbColumnInfo.DbType));
    }
}