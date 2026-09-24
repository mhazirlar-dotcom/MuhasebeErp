using System.Data;

namespace Accounting.Core.DataAccess.Persistence.Constants;

public static class SqlTypes
{
    #region Methods

    public static string GetSqlType(this SqlDbType dbType)
    {
        return dbType switch
        {
            SqlDbType.BigInt => "bigint",
            SqlDbType.Int => "int",
            SqlDbType.SmallInt => "smallint",
            SqlDbType.TinyInt => "tinyint",
            SqlDbType.Bit => "bit",
            SqlDbType.Char => "char",
            SqlDbType.Text => "text",
            SqlDbType.NText => "ntext",
            SqlDbType.Date => "date",
            SqlDbType.DateTime => "datetime",
            SqlDbType.DateTime2 => "datetime2",
            SqlDbType.SmallDateTime => "smalldatetime",
            SqlDbType.Float => "float",
            SqlDbType.Real => "real",
            SqlDbType.UniqueIdentifier => "uniqueidentifier",
            SqlDbType.Money => "money",
            SqlDbType.VarBinary => "varbinary(max)",
            SqlDbType.SmallMoney => "smallmoney",
            SqlDbType.Decimal => "decimal(18,2)",
            SqlDbType.VarChar => "varchar(max)",
            SqlDbType.NVarChar => "nvarchar(max)",
            _ => throw new NotSupportedException($"SqlDbType {dbType} için mapping yapılmadı.")
        };
    }

    public static string GetSqlType(this SqlDbType dbType , int length)
    {
        return dbType switch
        {
            SqlDbType.VarChar => $"varchar({length})",
            SqlDbType.NVarChar => $"nvarchar({length})",
            SqlDbType.Char => $"char({length})",
            _ => throw new NotSupportedException($"{dbType} için uzunluk parametresi desteklenmiyor.")
        };
    }

    public static string GetSqlType(this SqlDbType dbType , int precision , int scale)
    {
        return dbType switch
        {
            SqlDbType.Decimal => $"decimal({precision},{scale})",
            SqlDbType.Money => $"decimal({precision},{scale})",
            _ => throw new NotSupportedException($"{dbType} için precision/scale desteklenmiyor.")
        };
    }

    #endregion Methods
}