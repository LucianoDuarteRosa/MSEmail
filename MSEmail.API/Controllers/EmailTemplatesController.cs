using MediatR;
using Microsoft.AspNetCore.Mvc;
using MSEmail.Application.Commands.EmailTemplates;
using MSEmail.Application.DTOs;
using MSEmail.Application.Queries.EmailTemplates;

namespace MSEmail.API.Controllers;

/// <summary>
/// Controller para gerenciamento de templates de e-mail
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EmailTemplatesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EmailTemplatesController> _logger;

    public EmailTemplatesController(IMediator mediator, ILogger<EmailTemplatesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todos os templates de e-mail
    /// </summary>
    /// <param name="onlyActive">Se true, retorna apenas templates ativos</param>
    /// <returns>Lista de templates de e-mail</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EmailTemplateDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EmailTemplateDto>>> GetAllAsync([FromQuery] bool onlyActive = false)
    {
        _logger.LogInformation("Obtendo todos os templates de e-mail. OnlyActive: {OnlyActive}", onlyActive);

        var query = new GetAllEmailTemplatesQuery(onlyActive);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Obtém um template de e-mail por ID
    /// </summary>
    /// <param name="id">ID do template</param>
    /// <returns>Template de e-mail</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmailTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmailTemplateDto>> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Obtendo template de e-mail por ID: {Id}", id);

        var query = new GetEmailTemplateByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound($"Template com ID '{id}' não encontrado.");
        }

        return Ok(result);
    }

    /// <summary>
    /// Obtém um template de e-mail por nome
    /// </summary>
    /// <param name="name">Nome do template</param>
    /// <returns>Template de e-mail</returns>
    [HttpGet("by-name/{name}")]
    [ProducesResponseType(typeof(EmailTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmailTemplateDto>> GetByNameAsync(string name)
    {
        _logger.LogInformation("Obtendo template de e-mail por nome: {Name}", name);

        var query = new GetEmailTemplateByNameQuery(name);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound($"Template com nome '{name}' não encontrado.");
        }

        return Ok(result);
    }

    /// <summary>
    /// Cria um novo template de e-mail
    /// </summary>
    /// <param name="createDto">Dados do template a ser criado</param>
    /// <returns>Template criado</returns>
    [HttpPost]
    [ProducesResponseType(typeof(EmailTemplateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmailTemplateDto>> CreateAsync([FromBody] CreateEmailTemplateDto createDto)
    {
        _logger.LogInformation("Criando novo template de e-mail: {Name}", createDto.Name);

        try
        {
            var command = new CreateEmailTemplateCommand(createDto);
            var result = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao criar template");
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Atualiza um template de e-mail existente
    /// </summary>
    /// <param name="id">ID do template</param>
    /// <param name="updateDto">Dados atualizados do template</param>
    /// <returns>Template atualizado</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EmailTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmailTemplateDto>> UpdateAsync(Guid id, [FromBody] UpdateEmailTemplateDto updateDto)
    {
        if (id != updateDto.Id)
        {
            return BadRequest("ID da URL não confere com o ID do corpo da requisição.");
        }

        _logger.LogInformation("Atualizando template de e-mail: {Id}", id);

        try
        {
            var command = new UpdateEmailTemplateCommand(updateDto);
            var result = await _mediator.Send(command);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Template não encontrado para atualização");
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao atualizar template");
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Deleta um template de e-mail
    /// </summary>
    /// <param name="id">ID do template</param>
    /// <returns>Resultado da operação</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deletando template de e-mail: {Id}", id);

        var command = new DeleteEmailTemplateCommand(id);
        var result = await _mediator.Send(command);

        if (!result)
        {
            return NotFound($"Template com ID '{id}' não encontrado.");
        }

        return NoContent();
    }
}
