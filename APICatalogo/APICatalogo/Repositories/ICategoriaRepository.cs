using APICatalogo.Domains;

namespace APICatalogo.Repositories;

public interface ICategoriaRepository
{
   IEnumerable<Categoria> GetCategorias();
   
}