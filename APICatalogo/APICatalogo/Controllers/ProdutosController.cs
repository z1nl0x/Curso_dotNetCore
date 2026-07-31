using APICatalogo.Context;
using APICatalogo.Domains;
using APICatalogo.Repositories;
using Microsoft.AspNetCore.Mvc;
namespace APICatalogo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly IRepository<Produto> _repository;
    private readonly IProdutoRepository _produtoRepository;
    public ProdutosController(IRepository<Produto> repository, IProdutoRepository produtoRepository)
    {
        _repository = repository;
        _produtoRepository = produtoRepository;
    }

    [HttpGet("cat/{id}")]
    public ActionResult<IEnumerable<Produto>> GetProdutosCategoria([FromRoute] int id)
    {
        var produtos = _produtoRepository.GetProdutosPorCategoria(id);

        if (produtos is null)
        {
            return NotFound();
        }
        return Ok(produtos);
    }
    
    [HttpGet]
    public ActionResult<IEnumerable<Produto>> Get()
    {
        var produtos = _repository.GetAll();
        if (produtos == null)
        {
            return NotFound();
        }
        return Ok(produtos);
    }
    
    [HttpGet("{id:int}", Name="ObterProduto")]
    public ActionResult<Produto> Get(int id)
    {
        
        var produto = _produtoRepository.Get(p => p.ProdutoId == id);
        if (produto == null)
        {
            return NotFound("Produto não encontrado...");
        }

        return Ok(produto);
    }

    [HttpPost]
    public ActionResult<Produto> Post(Produto produto)
    {
        if (produto is null)
        {
            return BadRequest();
        }
        
        var novoProduto = _produtoRepository.Create(produto);
        
        return new CreatedAtRouteResult("ObterProduto", new { id = novoProduto.ProdutoId }, novoProduto);
    }

    [HttpPut("{id:int}")]
    public ActionResult<Produto> Put(int id, Produto produto)
    {
        if (id != produto.ProdutoId)
        {
            return BadRequest();
        }

        var produtoUpdate = _repository.Update(produto);

        return Ok(produtoUpdate);
    }

    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        var produtoDeletado = _repository.Get(p => p.ProdutoId == id);

        if (produtoDeletado is null)
        {
            return NotFound("Produto não encontrado!");
        }
        
        _repository.Delete(produtoDeletado);
        return Ok(produtoDeletado);
    }
}