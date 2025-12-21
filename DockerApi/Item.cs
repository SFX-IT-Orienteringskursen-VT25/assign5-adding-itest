using System;

public class Item
{
	public int Id { get; set; }
	public string Key { get; set; } = string.Empty;
	public int Value { get; set; }
	public Item() { }

	public Item(string key, int value)
	{
		Key = key;
		Value = value;
	}
	public Item(int id, string key, int value)
	{
		Id = id;
		Key = key;
		Value = value;
	}
}
