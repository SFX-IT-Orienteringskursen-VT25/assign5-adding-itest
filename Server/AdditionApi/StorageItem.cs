using System.ComponentModel.DataAnnotations;

namespace AdditionApi;

public class StorageItem
{
public int Id { get; set; }

[MaxLength(4000)]
public required string Key { get; set; }

public required string Value { get; set; }

}