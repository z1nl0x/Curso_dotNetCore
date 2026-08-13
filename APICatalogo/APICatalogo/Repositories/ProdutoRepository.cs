using APICatalogo.Context;
using APICatalogo.Domains;
using APICatalogo.Pagination;
using Microsoft.AspNetCore.Http.HttpResults;

namespace APICatalogo.Repositories;

public class ProdutoRepository : Repository<Produto>, IProdutoRepository
{
    public ProdutoRepository(AppDbContext context) : base(context)
    {
    }

    // public IEnumerable<Produto> GetProdutos(ProdutosParameters produtosParameters)
    // {
    //     return GetAll()
    //         .OrderBy(p => p.Nome)
    //         .Skip((produtosParameters.pageNumber -1) * produtosParameters.pageSize)
    //         .Take(produtosParameters.pageSize).ToList();
    // }

    public PagedList<Produto> GetProdutosFiltroPreco(ProdutosFiltroPreco produtosFiltroParams)
    {
        var produtos = GetAll().AsQueryable();

        if (produtosFiltroParams.Preco.HasValue && !string.IsNullOrEmpty(produtosFiltroParams.PrecoCriterio))
        {
            if (produtosFiltroParams.PrecoCriterio.Equals("maior", StringComparison.OrdinalIgnoreCase))
            {
                produtos = produtos.Where(p => p.Preco > produtosFiltroParams.Preco.Value).OrderBy(p => p.Preco);
            }
            else if (produtosFiltroParams.PrecoCriterio.Equals("menor", StringComparison.OrdinalIgnoreCase))
            {
                produtos = produtos.Where(p => p.Preco < produtosFiltroParams.Preco.Value).OrderBy(p => p.Preco);
            }
            else if (produtosFiltroParams.PrecoCriterio.Equals("igual", StringComparison.OrdinalIgnoreCase))
            {
                produtos = produtos.Where(p => p.Preco == produtosFiltroParams.Preco.Value).OrderBy(p => p.Preco);
            }
        }

        var produtosFiltrados =
            PagedList<Produto>.ToPagedList(produtos, produtosFiltroParams.pageNumber, produtosFiltroParams.PageSize);
        
        return produtosFiltrados;
    }

    public PagedList<Produto> GetProdutos(ProdutosParameters produtosParams)
    {
       var produtos = GetAll().OrderBy(produto => produto.ProdutoId).AsQueryable();
       var produtosOrdenados = PagedList<Produto>.ToPagedList(produtos, produtosParams.pageNumber, produtosParams.PageSize);
       return produtosOrdenados;
    }

    public IEnumerable<Produto> GetProdutosPorCategoria(int id)
    {
        return GetAll().Where(p => p.CategoriaId == id).ToList();
    }
}