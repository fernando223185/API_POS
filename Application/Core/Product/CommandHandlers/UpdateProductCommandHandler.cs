using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions.Catalogue;
using Application.Core.Product.Commands;
using Domain.Entities;
using MediatR;

namespace Application.Core.Product.CommandHandlers;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand , Products>
{
    private readonly IProductRepository _productRepository;
    public UpdateProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }
    public async Task<Products> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Products
        {
            ID = request.ID,
            name = request.name,
            description = request.description,
            code = request.code,
            barcode = request.barcode,
            category = request.category,
            branch = request.branch,
            price = request.price,
        };

        return await _productRepository.UpdateAsync(product);
    }
}

