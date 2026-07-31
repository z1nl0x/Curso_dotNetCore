using APICatalogo.Context;
using APICatalogo.Domains;
using Microsoft.AspNetCore.Http.HttpResults;

namespace APICatalogo.Repositories;

public class ProdutoRepository : Repository<Produto>, IProdutoRepository
{
    public ProdutoRepository(AppDbContext context) : base(context)
    {
    }

    public IEnumerable<Produto> GetProdutosPorCategoria(int id)
    {
        return GetAll().Where(p => p.CategoriaId == id).ToList();
    }
}