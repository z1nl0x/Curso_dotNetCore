using APICatalogo.Context;
using APICatalogo.Domains;
using APICatalogo.Filters;
using APICatalogo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    public CategoriasController(AppDbContext context, IConfiguration configuration, ILogger<CategoriasController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    // [HttpGet("LerArquivoConfiguracao")]
    // public string GetValores()
    // {
    //     var valor1 = _configuration["chave1"];
    //     var valor2 = _configuration["chave2"];
    //
    //     var secao1 = _configuration["secao1:chave2"];
    //
    //     return $"Chave1 = {valor1} \nChave2 = {valor2} \nSeção1 => Chave2 = {secao1}";
    // }
    
    
    [HttpGet]
    [ServiceFilter(typeof(ApiLoggingFilter))]
    public ActionResult<IEnumerable<Categoria>> Get()
    {
        var categorias =  _context.Categorias.AsNoTracking().ToList();
        if (categorias == null)
        {
            return NotFound();
        }
        return categorias;
    }
    
    // [HttpGet("UsandoFromServices/{nome}")]
    // public ActionResult<string> GetSaudacaoFromServices([FromServices] IMeuServico meuServico, string nome)
    // {
    //    return meuServico.Saudacao(nome);
    // }
    //
    // [HttpGet("SemUsarFromServices/{nome}")]
    // public ActionResult<string> GetSaudacaoSemFromServices(IMeuServico meuServico, string nome)
    // {
    //     return meuServico.Saudacao(nome);
    // }

    [HttpGet("produtos")]
    public ActionResult<IEnumerable<Categoria>> GetCategoriaProdutos()
    {
        _logger.LogInformation($"======================== GET CATEGORIAS PRODUTOS  ========================");
        var categorias = _context.Categorias.AsNoTracking().Include(p => p.Produtos).ToList();
        return categorias;
    }
    
    [HttpGet("{id:int}", Name="ObterCategoria")]
    public ActionResult<Categoria> Get(int id)
    {
        // throw new Exception("Exceção ao retornar a categoria por Id");

        // string[] teste = null;
        // if (teste.Length > 0)
        // {
        //     
        // }
        
        // _logger.LogInformation($"======================== GET api/categorias/id = {id}  ========================");
        
        var categoria = _context.Categorias.FirstOrDefault(c => c.CategoriaId == id);
        if (categoria == null)
        {
            return NotFound("Categoria não encontrada...");
        }
        
        return categoria;
    }

    [HttpPost]
    public ActionResult<Categoria> Post(Categoria categoria)
    {
        _context.Categorias.Add(categoria);
        _context.SaveChanges();
        return new CreatedAtRouteResult("ObterCategoria", new { id = categoria.CategoriaId }, categoria);
    }

    [HttpPut("{id:int}")]
    public ActionResult<Categoria> Put(int id, Categoria categoria)
    {
        if (id != categoria.CategoriaId)
        {
            return BadRequest();
        }

        _context.Entry(categoria).State = EntityState.Modified;
        _context.SaveChanges();
        
        return Ok(categoria);
    }

    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        var categoria = _context.Categorias.FirstOrDefault(c => c.CategoriaId == id);
        //var produto = _context.Produtos.Find(id);

        if (categoria == null)
        {
            return NotFound("Categoria não localizada...");
        }

        _context.Categorias.Remove(categoria);
        _context.SaveChanges();
        return Ok(categoria);
    }
}