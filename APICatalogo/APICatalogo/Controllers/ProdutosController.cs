using APICatalogo.Context;
using APICatalogo.Domains;
using APICatalogo.DTOs;
using APICatalogo.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
namespace APICatalogo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    // private readonly IRepository<Produto> _repository;
    // private readonly IProdutoRepository _produtoRepository;
    
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    
    public ProdutosController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        // _repository = repository;
        // _produtoRepository = produtoRepository;
        
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet("cat/{id}")]
    public ActionResult<IEnumerable<Produto>> GetProdutosCategoria([FromRoute] int id)
    {
        var produtos = _unitOfWork.ProdutoRepository.GetProdutosPorCategoria(id);

        if (produtos is null)
        {
            return NotFound();
        }
        return Ok(produtos);
    }
    
    [HttpGet]
    public ActionResult<IEnumerable<ProdutoDTO>> Get()
    {
        var produtos = _unitOfWork.ProdutoRepository.GetAll();
        if (produtos == null)
        {
            return NotFound();
        }
        
        var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);
        
        return Ok(produtosDto);
    }
    
    [HttpGet("{id:int}", Name="ObterProduto")]
    public ActionResult<ProdutoDTO> Get(int id)
    {
        
        var produto = _unitOfWork.ProdutoRepository.Get(p => p.ProdutoId == id);
        if (produto == null)
        {
            return NotFound("Produto não encontrado...");
        }
        
        var produtoDto = _mapper.Map<ProdutoDTO>(produto);

        return Ok(produto);
    }

    [HttpPost]
    public ActionResult<ProdutoDTO> Post(ProdutoDTO produtoDto)
    {
        if (produtoDto is null)
        {
            return BadRequest();
        }
        
        var produto = _mapper.Map<Produto>(produtoDto);
        
        var novoProduto = _unitOfWork.ProdutoRepository.Create(produto);
        _unitOfWork.Commit();
        
        var novoProdutoDto = _mapper.Map<ProdutoDTO>(novoProduto);
        
        return new CreatedAtRouteResult("ObterProduto", new { id = novoProdutoDto.ProdutoId }, novoProdutoDto);
    }

    [HttpPatch("{id}/UpdatePartial")]
    public ActionResult<ProdutoDTOUpdateResponse> Patch(int id, JsonPatchDocument<ProdutoDTOUpdateRequest> patchProdutoDto)
    {
        if (patchProdutoDto is null || id <= 0)
        {
            return BadRequest();
        }
        var produto = _unitOfWork.ProdutoRepository.Get(p => p.ProdutoId == id);
        if (produto is null)
        {
            return NotFound();
        }
        var produtoUpdateRequest = _mapper.Map<ProdutoDTOUpdateRequest>(produto);
        patchProdutoDto.ApplyTo(produtoUpdateRequest, ModelState);
        
        if(!ModelState.IsValid || !TryValidateModel(produtoUpdateRequest))
            return  BadRequest(ModelState);
        
        _mapper.Map(produtoUpdateRequest, produto);
        _unitOfWork.ProdutoRepository.Update(produto);
        _unitOfWork.Commit();
        
        return Ok(_mapper.Map<ProdutoDTOUpdateResponse>(produto));
    }

    [HttpPut("{id:int}")]
    public ActionResult<ProdutoDTO> Put(int id, ProdutoDTO produtoDto)
    {
        if (id != produtoDto.ProdutoId)
        {
            return BadRequest();
        }
        
        var produto = _mapper.Map<Produto>(produtoDto);

        var produtoUpdate = _unitOfWork.ProdutoRepository.Update(produto);
        _unitOfWork.Commit();
        
        var produtoAtualizadoDto = _mapper.Map<ProdutoDTO>(produtoUpdate);
        
        return Ok(produtoAtualizadoDto);
    }

    [HttpDelete("{id:int}")]
    public ActionResult<ProdutoDTO> Delete(int id)
    {
        var produtoDeletado = _unitOfWork.ProdutoRepository.Get(p => p.ProdutoId == id);

        if (produtoDeletado is null)
        {
            return NotFound("Produto não encontrado!");
        }
        
        _unitOfWork.ProdutoRepository.Delete(produtoDeletado);
        _unitOfWork.Commit();
        
        var produtoDeletadoDto = _mapper.Map<ProdutoDTO>(produtoDeletado);
        
        return Ok(produtoDeletadoDto);
    }
}