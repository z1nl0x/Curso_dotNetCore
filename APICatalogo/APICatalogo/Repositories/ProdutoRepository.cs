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