using System;
using System.Collections.Generic;
<<<<<<< HEAD
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Product.Commands
{
    public class UpdateProductCommand
    {
    }
=======
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Core.Product.Commands;

public class UpdateProductCommand : IRequest<Products>
{
    [Required]
    public int ID { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string code { get; set; }
    public string barcode { get; set; }
    public string category { get; set; }
    public string branch { get; set; }
    [Precision(18, 2)]
    public decimal price { get; set; }
>>>>>>> d399b8080a32d22d35314462b39a24df68edce47
}
