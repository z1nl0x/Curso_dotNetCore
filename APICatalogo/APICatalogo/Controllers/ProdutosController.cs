using APICatalogo.Context;
using APICatalogo.Domains;
using APICatalogo.DTOs;
using APICatalogo.Pagination;
using APICatalogo.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using X.PagedList;

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
    public async Task<ActionResult<IEnumerable<Produto>>> GetProdutosCategoria([FromRoute] int id)
    {
        var produtos = await _unitOfWork.ProdutoRepository.GetProdutosPorCategoriaAsync(id);

        if (produtos is null)
        {
            return NotFound();
        }
        return Ok(produtos);
    }

    [HttpGet("pagination")]
    public async  Task<ActionResult<IEnumerable<ProdutoDTO>>> GetProdutos([FromQuery] ProdutosParameters produtosParameters)
    {
        var produtos = await _unitOfWork.ProdutoRepository.GetProdutosAsync(produtosParameters);

        return ObterProtutos(produtos);
    }
    
    [HttpGet("filter/preco/pagination")]
    public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetProdutosFilterPreco([FromQuery] ProdutosFiltroPreco produtosFilterParameters)
    {
        var produtos = await _unitOfWork.ProdutoRepository.GetProdutosFiltroPrecoAsync(produtosFilterParameters);

        return ObterProtutos(produtos);
    }

    private ActionResult<IEnumerable<ProdutoDTO>> ObterProtutos(IPagedList<Produto> produtos)
    {
        var metadata = new
        {
            produtos.Count,
            produtos.PageSize,
            produtos.PageCount,
            produtos.TotalItemCount,
            produtos.PageNumber,
            produtos.HasNextPage,
            produtos.HasPreviousPage
        };
        
        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));
        
        var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);
        
        return Ok(produtosDto);
    }

    [HttpGet]
    [Authorize(Policy = "UserOnly")]
    public async Task<ActionResult<IEnumerable<ProdutoDTO>>> Get()
    {
        var produtos = await _unitOfWork.ProdutoRepository.GetAllAsync();
        if (produtos == null)
        {
            return NotFound();
        }
        
        var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);
        
        return Ok(produtosDto);
    }
    
    [HttpGet("{id:int}", Name="ObterProduto")]
    public async Task<ActionResult<ProdutoDTO>> Get(int id)
    {
        
        var produto = await _unitOfWork.ProdutoRepository.GetAsync(p => p.ProdutoId == id);
        if (produto == null)
        {
            return NotFound("Produto não encontrado...");
        }
        
        var produtoDto = _mapper.Map<ProdutoDTO>(produto);

        return Ok(produtoDto);
    }

    [HttpPost]
    public async Task<ActionResult<ProdutoDTO>> Post(ProdutoDTO produtoDto)
    {
        if (produtoDto is null)
        {
            return BadRequest();
        }
        
        var produto = _mapper.Map<Produto>(produtoDto);
        
        var novoProduto = _unitOfWork.ProdutoRepository.Create(produto);
        await _unitOfWork.CommitAsync();
        
        var novoProdutoDto = _mapper.Map<ProdutoDTO>(novoProduto);
        
        return new CreatedAtRouteResult("ObterProduto", new { id = novoProdutoDto.ProdutoId }, novoProdutoDto);
    }

    [HttpPatch("{id}/UpdatePartial")]
    public async Task<ActionResult<ProdutoDTOUpdateResponse>> Patch(int id, JsonPatchDocument<ProdutoDTOUpdateRequest> patchProdutoDto)
    {
        if (patchProdutoDto is null || id <= 0)
        {
            return BadRequest();
        }
        var produto = await _unitOfWork.ProdutoRepository.GetAsync(p => p.ProdutoId == id);
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
        await _unitOfWork.CommitAsync();
        
        return Ok(_mapper.Map<ProdutoDTOUpdateResponse>(produto));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProdutoDTO>> Put(int id, ProdutoDTO produtoDto)
    {
        if (id != produtoDto.ProdutoId)
        {
            return BadRequest();
        }
        
        var produto = _mapper.Map<Produto>(produtoDto);

        var produtoUpdate = _unitOfWork.ProdutoRepository.Update(produto);
        await _unitOfWork.CommitAsync();
        
        var produtoAtualizadoDto = _mapper.Map<ProdutoDTO>(produtoUpdate);
        
        return Ok(produtoAtualizadoDto);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ProdutoDTO>> Delete(int id)
    {
        var produtoDeletado = await _unitOfWork.ProdutoRepository.GetAsync(p => p.ProdutoId == id);

        if (produtoDeletado is null)
        {
            return NotFound("Produto não encontrado!");
        }
        
        _unitOfWork.ProdutoRepository.Delete(produtoDeletado);
        await _unitOfWork.CommitAsync();
        
        var produtoDeletadoDto = _mapper.Map<ProdutoDTO>(produtoDeletado);
        
        return Ok(produtoDeletadoDto);
    }
}