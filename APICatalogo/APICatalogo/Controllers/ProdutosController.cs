using APICatalogo.Context;
using APICatalogo.Domains;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    
    // [HttpGet("primeiro")]
    [HttpGet("{valor:alpha:length(5)}")]
    public ActionResult<Produto> GetPrimeiro()
    {
        var produto =  _context.Produtos.AsNoTracking().FirstOrDefault();
        if (produto == null)
        {
            return NotFound();
        }
        return produto;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Produto>>> Get()
    {
        var produtos =  _context.Produtos.AsNoTracking().ToListAsync();
        if (produtos == null)
        {
            return NotFound();
        }
        return await produtos;
    }
    
    [HttpGet("{id:int:min(1)}", Name="ObterProduto")]
    public async Task<ActionResult<Produto>> Get(int id)
    {
        
        var produto = _context.Produtos.AsNoTracking().FirstOrDefaultAsync(p => p.ProdutoId == id);
        if (produto == null)
        {
            return NotFound("Produto não encontrado...");
        }

        return await produto;
    }
    
    // [HttpGet("{id:int:min(1)}", Name="ObterProduto")]
    // public async Task<ActionResult<Produto>> Get(int id, [BindRequired] string nome)
    // {
    //     var nomeProduto = nome;
    //     
    //     var produto = _context.Produtos.AsNoTracking().FirstOrDefaultAsync(p => p.ProdutoId == id);
    //     if (produto == null)
    //     {
    //         return NotFound("Produto não encontrado...");
    //     }
    //
    //     return await produto;
    // }

    [HttpPost]
    public ActionResult<Produto> Post(Produto produto)
    {
        _context.Produtos.Add(produto);
        _context.SaveChanges();
        return new CreatedAtRouteResult("ObterProduto", new { id = produto.ProdutoId }, produto);
    }

    [HttpPut("{id:int}")]
    public ActionResult<Produto> Put(int id, Produto produto)
    {
        if (id != produto.ProdutoId)
        {
            return BadRequest();
        }

        _context.Entry(produto).State = EntityState.Modified;
        _context.SaveChanges();
        
        return Ok(produto);
    }

    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        var produto = _context.Produtos.FirstOrDefault(p => p.ProdutoId == id);
        //var produto = _context.Produtos.Find(id);

        if (produto == null)
        {
            return NotFound("Produto não localizado...");
        }

        _context.Produtos.Remove(produto);
        _context.SaveChanges();
        return Ok(produto);
    }
}