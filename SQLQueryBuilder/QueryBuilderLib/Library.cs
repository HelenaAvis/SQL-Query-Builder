namespace QueryBuilderLib
{
    public class Query
    {
        public List<(String name, String? alias, String? table)> columns;
        public (String name, String? alias) table;
        public List<Join> joins;
        public List<Condition> conditions;

        public Query() {
            columns = new List<(String, String?, String?)>();
            joins = new List<Join>();
            conditions = new List<Condition>();
        }

        /// <summary>
        /// Prints a column tuple into valid SQL in String format
        /// </summary>
        /// <param name="column">A tuple representing the column.</param>
        /// <returns>A string representing the column in SQL format.</returns>
        public static String PrintColumn((String name, String? alias, String? table) column)
        {
            String tableString;
            if (column.table is null)
            {
                tableString = "";
            } else
            {
                tableString = $"{column.table}.";
            }

            String aliasString;
            if (column.alias is null)
            {
                aliasString = "";
            }
            else
            {
                aliasString = $" AS {column.alias}";
            }
            return $"{tableString}{column.name}{aliasString}";
        }

        /// <summary>
        /// Prints the list of columns in the query into a valid SQL SELECT clause.
        /// </summary>
        /// <returns>A string representing the SELECT clause in SQL format.</returns>
        public String PrintColumnList()
        {
            if (this.columns.Count == 0)
            {
                return "*";
            } else
            { 
                return String.Join(", ", this.columns.Select(c => PrintColumn(c)));
            }
        }

        /// <summary>
        /// Prints the list of joins in the query into a valid SQL JOIN clause.
        /// </summary>
        /// <returns>A string representing the JOIN clause in SQL format.</returns>
        public String PrintJoinList()
        {
            return String.Join(" ", this.joins.Select(j => j.PrintJoin()));
        }

        /// <summary>
        /// Prints the list of conditions in the query into a valid SQL WHERE clause.
        /// </summary>
        /// <returns>A string representing the WHERE clause in SQL format.</returns>
        public String PrintConditionList()
        {
            return String.Join(" ", this.conditions.Select(c => c.PrintCondition()));
        }

        /// <summary>
        /// Prints the Query object as a full SQL query.
        /// </summary>
        /// <returns>The full SQL query as a String.</returns>
        public override String? ToString()
        {
            String selectClause = $"SELECT {this.PrintColumnList()} ";
            String fromClause;
            if (this.table.alias is null)
            {
                fromClause = $"FROM {this.table.name}";
            } else
            {
                fromClause = $"FROM {this.table.name} {this.table.alias}";
            }

            String joinClause = $" {this.PrintJoinList()}";
            String whereClause = $" WHERE {this.PrintConditionList()}";

            return $"{selectClause}{fromClause}{joinClause}{whereClause};";
        }

        /// <summary>
        /// Check if a table has been referenced or aliased in the query.
        /// </summary>
        /// <param name="table">The table to check for.</param>
        /// <returns>True if the table exists in the query, false otherwise.</returns>
        public bool TableExists((String name, String? alias) table)
        {
            // Check if the primary table being selected is a match
            if (this.table.name == table.name || this.table.alias == table.alias)
            {
                return true;
            }

            // Check if any of the tables being joined are a match    
            foreach (Join join in joins)
            {
                if (join.rightTable.Item1 == table.name || join.rightTable.Item2 == table.alias)
                {
                    return true;
                }
            }

            // There are no matches found
            return false;
        }
       
        /// <summary>
        /// Add a column to be selected in the query.
        /// </summary>
        /// <param name="column">The column to select</param>
        public void AddSelectColumn(String column)
        {
            columns.Add((column, null, null));
        }

        /// <summary>
        /// Add a column to be selected in the query and alias it.
        /// </summary>
        /// <param name="column">The column to select.</param>
        /// <param name="alias">The alias for the column.</param>
        public void AddSelectColumnWithAlias(String column, String alias)
        {
            columns.Add((column, alias, null));
        }

        /// <summary>
        /// Add a column to be selected in the query and specify a table.
        /// </summary>
        /// <param name="table">The table the column is in.</param>
        /// <param name="column">The column to select.</param>
        /// <exception cref="TableNotFoundException">Thrown if the table cannot be found in the query.</exception>
        public void AddSelectColumnWithTable(String table, String column)
        {
            // Check if the table has been referenced or aliased in the query
            if (!TableExists((table, table)))
            {
                throw new TableNotFoundException($"Table '{table}' not found in the query.");
            }
            columns.Add((column, null, table));
        }

        /// <summary>
        /// Add a column to be selected in the query, specify a table, and alias it.
        /// </summary>
        /// <param name="table">The table the column is in.</param>
        /// <param name="column">The column to select.</param>
        /// <param name="alias">The alias for the column.</param>
        /// <exception cref="TableNotFoundException">Thrown if the table cannot be found in the query.</exception>
        public void AddSelectColumnWithTableAndAlias(String table, String column, String alias)
        {
            // Check if the table has been referenced or aliased in the query
            if (!TableExists((table, table)))
            {
                throw new TableNotFoundException($"Table '{table}' not found in the query.");
            }
            columns.Add((column, alias, table));
        }

        /// <summary>
        /// Sets the FROM table for the query (the primary table being queried).
        /// </summary>
        /// <param name="table">The table to query.</param>
        public void SetFromTable(String table)
        {
            this.table = (table, null);
        }

        /// <summary>
        /// Sets the FROM table for the query (the primary table being queried) with an alias.
        /// </summary>
        /// <param name="table">The table to query.</param>
        /// <param name="alias">The alias for the table.</param>
        public void SetFromTable(String table, String alias)
        {
            this.table = (table, alias);
        }

        /// <summary>
        /// Adds a join to the query.
        /// </summary>
        /// <param name="join">The new join to add.</param>
        /// <exception cref="TableNotFoundException">Thrown if the left table in the join has not been referenced or aliased in the query.</exception>
        public void AddJoin(Join join)
        {
            if (!TableExists((join.leftTable, join.leftTable)))
            {
                throw new TableNotFoundException($"Table '{join.leftTable}' not found in the query.");
            }

            joins.Add(join);
        }

        /// <summary>
        /// Adds a WHERE condition to the query.
        /// </summary>
        /// <param name="condition">The new condition to add.</param>
        /// <exception cref="UnspecifiedConditionTypeException">If this is not the first condition added, and the condition type (AND/OR) is not specified.</exception>
        /// <exception cref="TableNotFoundException">Thrown if the table in the condition has not been referenced or aliased in the query.</exception>
        /// <exception cref="ArgumentNullException">Thrown if the condition value is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the condition value is not a string or an integer.</exception>
        public void AddCondition(Condition condition) 
        {
            // Check if this is the first condition being added. If not, ensure that the condition type is specified.
            if (this.conditions.Count > 0 && condition.type == ConditionType.None)
            {
                throw new UnspecifiedConditionTypeException("Condition type must be specified for all conditions after the first.");
            }

            // If a table and field are used in condition, check if the table in the condition has been referenced or aliased in the query.
            if (condition.table is not null && !TableExists((condition.table, condition.table)))
            {
                throw new TableNotFoundException($"Table '{condition.table}' not found in the query.");
            }

            // Check if the value is null
            if (condition.value is null)
            {
                throw new ArgumentNullException("Condition value cannot be null.");
            }

            // Check if the value is a string or an integer
            if (!(condition.value is String) && !(condition.value is Int32))
            {
                throw new ArgumentException("Condition value must be a string or an integer.");
            }

            this.conditions.Add(condition);
        }
    }

    public abstract class Join
    {
        public String leftTable;
        public (String name, String? alias) rightTable;
        public String leftField;
        public String rightField;

        public Join(String leftTable, (String name, String? alias) rightTable, String leftField, String rightField)
        {
            this.leftTable = leftTable;
            this.rightTable = rightTable;
            this.leftField = leftField;
            this.rightField = rightField;
        }

        public abstract String PrintJoin();
    }

    public class OuterJoin : Join
    {
        public OuterJoin(String leftTable, (String name, String? alias)  rightTable, String leftField, String rightField) : base(leftTable, rightTable, leftField, rightField) {}

        /// <summary>
        /// Converts the given OuterJoin object into an SQL OUTER JOIN clause.
        /// </summary>
        /// <param name="join">The OuterJoin object to convert.</param>
        /// <returns>The SQL OUTER JOIN clause as a String.</returns>
        public override String PrintJoin()
        {
            if (this.rightTable.Item2 is null)
            {
                return $"OUTER JOIN {this.rightTable.Item1} ON {this.leftTable}.{this.leftField} = {this.rightTable.Item1}.{this.rightField}";
            } else
            {
                return $"OUTER JOIN {this.rightTable.Item1} {this.rightTable.Item2} ON {this.leftTable}.{this.leftField} = {this.rightTable.Item2}.{this.rightField}";
            }
        }
    }

    public class InnerJoin : Join
    {
        public InnerJoin(String leftTable, (String name, String? alias) rightTable, String leftField, String rightField) : base(leftTable, rightTable, leftField, rightField) { }

        /// <summary>
        /// Converts the given InnerJoin object into an SQL INNER JOIN clause.
        /// </summary>
        /// <param name="join">The InnerJoin object to convert.</param>
        /// <returns>The SQL INNER JOIN clause as a String.</returns>
        public override String PrintJoin()
        {
            if (this.rightTable.Item2 is null)
            {
                return $"INNER JOIN {this.rightTable.Item1} ON {this.leftTable}.{this.leftField} = {this.rightTable.Item1}.{this.rightField}";
            }
            else
            {
                return $"INNER JOIN {this.rightTable.Item1} {this.rightTable.Item2} ON {this.leftTable}.{this.leftField} = {this.rightTable.Item2}.{this.rightField}";
            }
        }
    }

    public class Condition
    {
        public ConditionType type;
        public String? table;
        public String field;
        public ComparisonOperator comparisonOperator;
        public Object value;

        // Constructors to use the table name/alias and field
        public Condition(String table, String field, ComparisonOperator comparisonOperator, Object value)
        {
            this.type = ConditionType.None;
            this.table = table;
            this.field = field;
            this.comparisonOperator = comparisonOperator;
            this.value = value;
        }

        public Condition (ConditionType type, String table, String field, ComparisonOperator comparisonOperator, Object value)
        {
            this.type = type;
            this.table = table;
            this.field = field;
            this.comparisonOperator = comparisonOperator;
            this.value = value;
        }

        // Constructors to use a column name/alias
        public Condition(String field, ComparisonOperator comparisonOperator, Object value)
        {
            this.type = ConditionType.None;
            this.field = field;
            this.comparisonOperator = comparisonOperator;
            this.value = value;
        }

        public Condition (ConditionType type, String field, ComparisonOperator comparisonOperator, Object value)
        {
            this.type = type;
            this.field = field;
            this.comparisonOperator = comparisonOperator;
            this.value = value;
        }

        /// <summary>
        /// Prints the condition to a String.
        /// </summary>
        /// <returns>A valid SQL WHERE condition as a String.</returns>
        public String PrintCondition()
        {
            String typeString;
            switch (this.type)
            {
                case ConditionType.And:
                    typeString = "AND ";
                    break;
                case ConditionType.Or:
                    typeString = "OR ";
                    break;
                default:
                    typeString = "";
                    break;
            }

            String tableString;
            if (this.table is null)
            {
                tableString = "";
            }
            else
            {
                tableString = $"{this.table}.";
            }

            String operatorString = "";
            switch (this.comparisonOperator)
            {
                case ComparisonOperator.Equal:
                    operatorString = "=";
                    break;
                case ComparisonOperator.NotEqual:
                    operatorString = "<>";
                    break;
                case ComparisonOperator.GreaterThanEqualTo:
                    operatorString = ">=";
                    break;
                case ComparisonOperator.LessThanEqualTo:
                    operatorString = "<=";
                    break;
                case ComparisonOperator.GreaterThan:
                    operatorString = ">";
                    break;
                case ComparisonOperator.LessThan:
                    operatorString = "<";
                    break;
            }

            if (this.value is String)
            {
                return $"{typeString}{tableString}{this.field} {operatorString} '{this.value}'";
            }
            else
            {
                return $"{typeString}{tableString}{this.field} {operatorString} {this.value}";
            }
        }
    }

    public enum ConditionType
    {
        And, 
        Or,
        None
    }

    public enum ComparisonOperator
    {
        Equal,
        NotEqual,
        GreaterThanEqualTo,
        LessThanEqualTo,
        GreaterThan,
        LessThan
    }

    [Serializable]
    public class TableNotFoundException : Exception
    {
        public TableNotFoundException() : base() {}
        public TableNotFoundException(string message) : base(message) {}
        public TableNotFoundException(string message, Exception inner) : base(message, inner) {}
    }

    [Serializable]
    public class UnspecifiedConditionTypeException : Exception
    {
        public UnspecifiedConditionTypeException() : base() { }
        public UnspecifiedConditionTypeException(string message) : base(message) { }
        public UnspecifiedConditionTypeException(string message, Exception inner) : base(message, inner) { }
    }
}
