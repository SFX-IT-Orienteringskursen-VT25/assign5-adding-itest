using AdditionApi;
using AdditionApi.Repository;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]

public class AdditionController : ControllerBase
{
    private IAdditionRepository _additionRepository;

    public AdditionController(IAdditionRepository additionRepository)
    {
        _additionRepository = additionRepository;
    }

    [HttpGet("NumberList")]
    public ActionResult<List<string>> GetNumbers()
    {
        var results = _additionRepository.Select();
        return Ok(results);
    }

    [HttpPost]
    public IActionResult InsertNumber([FromBody] string value)
    {
        if (!int.TryParse(value, out _))
        {
            return BadRequest("Value must be a valid integer.");
        }
        _additionRepository.InsertValue(value);
        return Ok();
    }

    [HttpGet("TotalSum")]
    public ActionResult<string> GetTotal()
    {
        var results = _additionRepository.Select();
        if(results == null || results.Count == 0)
        {
            return Ok("0");
        }
        else
        {
            string sum = results.Sum(x => int.Parse(x)).ToString();
            return Ok(sum);
        }
    }

    [HttpDelete("DeleteAll")]
    public IActionResult DeleteAllNumbers()
    {
        _additionRepository.DeleteAll();
        return Ok();
    }
}
