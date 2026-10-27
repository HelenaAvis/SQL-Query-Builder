using QueryBuilderLib;

namespace ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Example usage of the QueryBuilderLib:

            // SELECT * FROM Users WHERE Age > 30 AND Name = 'John'
            Query ex1 = new Query();
            ex1.SetFromTable("Users");

            ex1.AddCondition(new Condition("Age", ComparisonOperator.GreaterThan, 30));
            ex1.AddCondition(new Condition(ConditionType.And, "Name", ComparisonOperator.Equal, "John"));

            Console.WriteLine(ex1.ToString() + "\n");

            // SELECT Users.Name, Orders.OrderDate FROM Users INNER JOIN Orders ON Users.UserID = Orders.UserID WHERE Orders.OrderDate > '2021-01-01'
            Query ex2 = new Query();
            ex2.SetFromTable("Users");
            ex2.AddSelectColumnWithTable("Users", "Name");
            ex2.AddJoin(new InnerJoin("Users", ("Orders", null), "UserID", "UserID"));
            ex2.AddSelectColumnWithTable("Orders", "OrderDate");
            ex2.AddCondition(new Condition("Orders.OrderDate", ComparisonOperator.GreaterThan, "2021-01-01"));

            Console.WriteLine(ex2.ToString() + "\n");

            // SELECT U.Name, O.OrderDate FROM Users U INNER JOIN Orders O ON U.UserID = O.UserID WHERE O.OrderDate > '2021-01-01'
            Query ex3 = new Query();
            ex3.SetFromTable("Users", "U");
            ex3.AddSelectColumnWithTable("U", "Name");
            ex3.AddJoin(new InnerJoin("U", ("Orders", "O"), "UserID", "UserID"));
            ex3.AddSelectColumnWithTable("O", "OrderDate");
            ex3.AddCondition(new Condition("O.OrderDate", ComparisonOperator.GreaterThan, "2021-01-01"));
            Console.WriteLine(ex3.ToString());
        }
    }
}
