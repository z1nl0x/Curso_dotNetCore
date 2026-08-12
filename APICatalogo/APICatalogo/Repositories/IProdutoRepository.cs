using APICatalogo.Domains;
using APICatalogo.Pagination;

namespace APICatalogo.Repositories;

public interface IProdutoRepository : IRepository<Produto>
{
    // IEnumerable<Produto> GetProdutos(ProdutosParameters produtosParameters);
    PagedList<Produto> GetProdutos(ProdutosParameters produtosParameters);
    IEnumerable<Produto> GetProdutosPorCategoria(int id);
}