using APICatalogo.Context;
using APICatalogo.Domains;
using Microsoft.AspNetCore.Mvc;

namespace APICatalogo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly AppDbContext _context;
    public ProdutosController(AppDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public ActionResult<IEnumerable<Produto>> Get()
    {
        var produtos =  _context.Produtos.ToList();
        if (produtos == null)
        {
            return NotFound();
        }
        return produtos;
    }
    
    [HttpGet("{id:int}", Name="ObterProduto")]
    public ActionResult<Produto> Get(int id)
    {
        var produto = _context.Produtos.FirstOrDefault(p => p.ProdutoId == id);
        if (produto == null)
        {
            return NotFound("Produto não encontrado...");
        }

        return produto;
    }

    [HttpPost]
    public ActionResult<Produto> Post(Produto produto)
    {
        _context.Produtos.Add(produto);
        _context.SaveChanges();
        return new CreatedAtRouteResult("ObterProduto", new { id = produto.ProdutoId }, produto);
    }
}