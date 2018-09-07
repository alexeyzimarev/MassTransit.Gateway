namespace MassTransit.Gateway.SqlServer
{
    public static class SqlConstants
    {
        public const string DequeueQuery = @"
WITH data AS (SELECT TOP(1) * FROM {0} WITH (UPDLOCK, READPAST, ROWLOCK) ORDER BY RowNumber)
DELETE FROM data OUTPUT DELETED.*";
    }
}