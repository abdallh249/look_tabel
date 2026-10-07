using look.Models;
using look.repository;

var repository = new Repository();

var sale1 = new Sales
{
    Id = 1,
    Amount = 100m
};

var sale2 = new Sales
{
    Id = 2,
    Amount = 200m
};

var sale3 = new Sales
{
    Id = 3,
    Amount = 300m
};

await repository.AddSale(sale1);
await repository.AddSale(sale2);
await repository.AddSale(sale3);


Console.WriteLine("Sales added successfully.");

await repository.PayTax();

Console.WriteLine("Tax paid successfully.");