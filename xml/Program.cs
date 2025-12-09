using System;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;

public class Program
{
	// This is the main entry point of the application.
	public static int Main(string[] args)
	{
		var mp = new MainPage();
		mp.Start();

		return 0;
	}
}

// Model classes for XML serialization testing
public class SampleDataModel
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public DateTime CreatedDate { get; set; }
	public decimal Price { get; set; }
	public bool IsActive { get; set; }
	public List<string> Tags { get; set; } = new();
	public Dictionary<string, object> Properties { get; set; } = new();
	public NestedObject Details { get; set; } = new();

	public string Serialize()
	{
		var xe = new XElement("SampleData",
			new XElement("Id", Id),
			new XElement("Name", Name),
			new XElement("CreatedDate", CreatedDate.ToString("O")),
			new XElement("Price", Price),
			new XElement("IsActive", IsActive),
			new XElement("Tags", Tags.Select(t => new XElement("Tag", t))),
			new XElement("Properties", Properties.Select(p => new XElement("Property",
				new XAttribute("Key", p.Key),
				new XAttribute("Value", p.Value?.ToString() ?? "")))),
			new XElement("Details", Details.Serialize())
		);

		using (var sw = new StringWriter())
		{
			var settings = new XmlWriterSettings { CheckCharacters = false };
			using (var xw = XmlWriter.Create(sw, settings))
				xe.WriteTo(xw);
			return sw.ToString();
		}
	}
}

public class NestedObject
{
	public string Description { get; set; } = string.Empty;
	public int Priority { get; set; }
	public List<SubItem> Items { get; set; } = new();

	public XElement Serialize()
	{
		return new XElement("NestedObject",
			new XElement("Description", Description),
			new XElement("Priority", Priority),
			new XElement("Items", Items.Select(i => i.Serialize()))
		);
	}
}

public class SubItem
{
	public string Code { get; set; } = string.Empty;
	public double Value { get; set; }
	public string Category { get; set; } = string.Empty;

	public XElement Serialize()
	{
		return new XElement("SubItem",
			new XElement("Code", Code),
			new XElement("Value", Value),
			new XElement("Category", Category)
		);
	}
}

public class MainPage
{
	private List<SampleDataModel> _xmlTestData = new();

	public MainPage()
	{
		_xmlTestData = new List<SampleDataModel>();
		var random = new Random();
		var categories = new[] { "Electronics", "Books", "Clothing", "Home", "Sports", "Toys", "Food", "Health" };
		var descriptions = new[] {
			"High-quality product with excellent features",
			"Premium item for discerning customers",
			"Essential everyday item",
			"Professional grade equipment",
			"Budget-friendly option",
			"Luxury premium product"
		};

		// Create 40000 complex objects for XML serialization
		for (int i = 1; i <= 40000; i++)
		{
			var model = new SampleDataModel
			{
				Id = i,
				Name = $"Product {i:D4}",
				CreatedDate = DateTime.Now.AddDays(-random.Next(0, 365)),
				Price = (decimal)(random.NextDouble() * 1000),
				IsActive = random.Next(0, 2) == 1,
				Tags = Enumerable.Range(0, random.Next(3, 8))
					.Select(_ => categories[random.Next(categories.Length)])
					.Distinct()
					.ToList()
			};

			// Add complex properties
			for (int j = 0; j < random.Next(5, 15); j++)
			{
				model.Properties[$"Property_{j}"] = random.Next(0, 3) switch
				{
					0 => random.Next(1, 1000),
					1 => $"StringValue_{random.Next(1, 100)}",
					_ => random.NextDouble() * 100
				};
			}

			// Add nested object
			model.Details = new NestedObject
			{
				Description = descriptions[random.Next(descriptions.Length)],
				Priority = random.Next(1, 11),
				Items = Enumerable.Range(0, random.Next(5, 20))
					.Select(k => new SubItem
					{
						Code = $"CODE_{random.Next(1000, 9999)}",
						Value = random.NextDouble() * 1000,
						Category = categories[random.Next(categories.Length)]
					}).ToList()
			};

			_xmlTestData.Add(model);
		}
	}

	private (TimeSpan, int) StartXmlSerializationWork()
	{
		var sw = Stopwatch.StartNew();

		var xmlResults = _xmlTestData.Select(x => x.Serialize()).ToList();
		var totalLength = xmlResults.Sum(xml => xml.Length);

		sw.Stop();
		return (sw.Elapsed, totalLength);
	}

	public void Start()
	{
		StartXmlSerializationWork();
	}
}
