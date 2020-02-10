using System;
using System.Collections.Generic;
using JournalCli.Core;
using Xunit;
using System.Linq;
using FluentAssertions;
using JournalCli.Infrastructure;
using NodaTime;

// ReSharper disable StringLiteralTypo

namespace JournalCli.Tests
{
    public class JournalEntryBodyTests
    {
        
        [Fact]
        public void This_ParsesBodyCorrectly_WhenTopElementHasNoHeader()
        {
            var rawBody = TestBodies.OnlyFirstParagraphHasNoHeader;
            var body = new JournalEntryBody(rawBody);
            body.Count().Should().Be(4);
            const string expectedText = "Contrary to popular belief, Lorem Ipsum is not simply random text. It has roots in a piece of classical Latin literature from 45 BC, making  it over 2000 years old. Richard McClintock, a Latin professor at  Hampden-Sydney College in Virginia, looked up one of the more obscure  Latin words, consectetur, from a Lorem Ipsum passage, and going through  the cites of the word in classical literature, discovered the  undoubtable source. Lorem Ipsum comes from sections 1.10.32 and 1.10.33  of \"de Finibus Bonorum et Malorum\" (The Extremes of Good and Evil) by  Cicero, written in 45 BC. This book is a treatise on the theory of  ethics, very popular during the Renaissance. The first line of Lorem  Ipsum, \"Lorem ipsum dolor sit amet..\", comes from a line in section  1.10.32.\r\n\r\nThe standard chunk of Lorem Ipsum used since the 1500s is reproduced below for those interested. Sections 1.10.32 and 1.10.33  from \"de Finibus Bonorum et Malorum\" by Cicero are also reproduced in  their exact original form, accompanied by English versions from the 1914 translation by H. Rackham.";
            body.Single(x => x.header == "## Where does it come from?").text.Should().Be(expectedText);
            body.First().header.Should().BeEmpty();
            body.First().text.Should().Be("**Lorem Ipsum** is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard  dummy text ever since the 1500s, when an unknown printer took a galley  of type and scrambled it to make a type specimen book. It has survived  not only five centuries, but also the leap into electronic typesetting,  remaining essentially unchanged. It was popularised in the 1960s with  the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker  including versions of Lorem Ipsum.");
            var output = body.ToString();
            output.Should().Be(rawBody.Trim());
        }

        [Fact]
        public void This_ParsesBodyCorrectly_WhenAllElementsHaveHeaders()
        {
            var rawBody = TestBodies.AllTextHasExactlyOneHeader;
            var body = new JournalEntryBody(rawBody);
            body.Count().Should().Be(5);
            body.Should().OnlyContain(x => !string.IsNullOrWhiteSpace(x.header) && !string.IsNullOrWhiteSpace(x.text));
            body.First(x => x.header == "## 1914 translation by H. Rackham").text.Should().Be("\"But I must explain to you how all this mistaken idea of denouncing  pleasure and praising pain was born and I will give you a complete  account of the system, and expound the actual teachings of the great  explorer of the truth, the master-builder of human happiness. No one  rejects, dislikes, or avoids pleasure itself, because it is pleasure,  but because those who do not know how to pursue pleasure rationally  encounter consequences that are extremely painful. Nor again is there  anyone who loves or pursues or desires to obtain pain of itself, because it is pain, but because occasionally circumstances occur in which toil  and pain can procure him some great pleasure. To take a trivial example, which of us ever undertakes laborious physical exercise, except to  obtain some advantage from it? But who has any right to find fault with a man who chooses to enjoy a pleasure that has no annoying consequences,  or one who avoids a pain that produces no resultant pleasure?\"");
            var output = body.ToString();
            output.Should().Be(rawBody.Trim());
        }

        [Fact]
        public void This_ParsesBodyCorrectly_WhenNoHeadersExist()
        {
            var rawBody = TestBodies.NoHeadersOnlyText;

            var body = new JournalEntryBody(rawBody);
            body.Count().Should().Be(1);
            body.Single(x => x.header == string.Empty).text.Should().Be(rawBody.Trim());
            var output = body.ToString();
            output.Should().Be(rawBody.Trim());
        }

        [Fact]
        public void This_ParsesBodyCorrectly_WhenStandardHeaderLayoutPresent()
        {
            var rawBody = TestBodies.MultipleHeadersIncludingDefaultH1;

            var body = new JournalEntryBody(rawBody);
            body.Count().Should().Be(6);
            body.Should().OnlyContain(x => !string.IsNullOrWhiteSpace(x.header) && !string.IsNullOrWhiteSpace(x.text));
            var output = body.ToString();
            output.Should().Be(rawBody.Trim());
        }

        [Fact]
        public void This_ParsesBodyCorrectly_WhenMultipleHeadersHaveNoText()
        {
            var rawBody = TestBodies.MultipleHeadersWithoutAssociatedText;

            var body = new JournalEntryBody(rawBody);
            body.Count().Should().Be(4);
            body.First().text.Should().BeEmpty();
            body.Take(2).Skip(1).Take(1).Should().OnlyContain(x => x.text == string.Empty && !string.IsNullOrWhiteSpace(x.header));
            body.Skip(2).Take(1).Should().OnlyContain(x => !string.IsNullOrWhiteSpace(x.header) && !string.IsNullOrWhiteSpace(x.text));
            var output = body.ToString();
            output.Should().Be(rawBody.Trim());
        }

        [Fact]
        public void This_ParsesCorrectly_WhenBodyIsEmpty()
        {
            var rawBody = string.Empty;
            var body = new JournalEntryBody(rawBody);
            body.Count().Should().Be(0);
            var output = body.ToString();
            output.Should().Be(rawBody.Trim());
        }

        [Theory]
        [InlineData("1")]
        [InlineData("a")]
        [InlineData("$")]
        [InlineData("#")]
        public void This_ParsesCorrectly_WhenBodyHasASingleNonHeaderCharacter(string rawBody)
        {
            var body = new JournalEntryBody(rawBody);
            body.Count().Should().Be(1);
            body.Count(x => string.IsNullOrEmpty(x.header)).Should().Be(1);
            body.Count(x => !string.IsNullOrEmpty(x.text)).Should().Be(1);
            var output = body.ToString();
            output.Should().Be(rawBody.Trim());
        }

        [Theory]
        [InlineData("#")]
        [InlineData("##")]
        [InlineData("### ")]
        public void This_ParsesCorrectly_WhenBodyOnlyHasHeaderMarkers(string rawBody)
        {
            var body = new JournalEntryBody(rawBody);
            body.Count().Should().Be(1);
            body.Count(x => string.IsNullOrEmpty(x.header)).Should().Be(1);
            body.Count(x => !string.IsNullOrEmpty(x.text)).Should().Be(1);
            var output = body.ToString();
            output.Should().Be(rawBody.Trim());
        }

        [Fact]
        public void This_ParsesCorrectly_WhenBodyHasNestedHeaders()
        {
            var rawBody = TestBodies.NestedHeaders;

            var body = new JournalEntryBody(rawBody);
            body.Count().Should().Be(6);
            body.Count(x => string.IsNullOrEmpty(x.text)).Should().Be(2);
            body.Count(x => string.IsNullOrEmpty(x.header)).Should().Be(0);
            var output = body.ToString();
            output.Should().Be(rawBody.Trim());
        }

        [Fact]
        public void AddOrAppendToDefaultHeader_AppendsHeader_WhenExists()
        {
            var rawBody = TestBodies.MultipleHeadersIncludingDefaultH1;

            var body = new JournalEntryBody(rawBody);
            var originalCount = body.Count();
            body.Count(x => DateTime.TryParse(x.header.Replace("#", "").Trim(), out _)).Should().Be(1);
            var defaultHeader = body.First().header;
            var originalIndex = body.ToList().FindIndex(x => x.header == defaultHeader);
            const string appended = "- Today I did the thing\r\n- Then I did another thing.";
            body.AddOrAppendToDefaultHeader(Today.Date(), new[] { appended });

            var currentIndex = body.ToList().FindIndex(x => x.header == defaultHeader);
            currentIndex.Should().Be(originalIndex);

            var currentCount = body.Count();
            currentCount.Should().Be(originalCount);  

            var output = body.ToString();
            output.Length.Should().Be(rawBody.Trim().Length + appended.Length + 4); // 2 line breaks X 2 chars each
        }

        [Fact]
        public void AddOrAppendToDefaultHeader_AddsHeader_WhenNotExists()
        {
            var rawBody = TestBodies.AllTextHasExactlyOneHeader;

            var body = new JournalEntryBody(rawBody);
            body.Count().Should().Be(5);
            const string appended = "- Today I did the thing\r\n- Then I did another thing.";
            var date = new LocalDate(2020, 2, 9);
            body.AddOrAppendToDefaultHeader(date, new[] { appended });

            body.ToList().FindIndex(x => DateTime.TryParse(x.header.Replace("#", "").Trim(), out _)).Should().Be(0);
            body.Count().Should().Be(6);

            var output = body.ToString();
            output.Length.Should().Be(rawBody.Trim().Length + appended.Length + $"# {date.ToString()}".Length + 8); // 2 line breaks X 2 chars each
        }

        [Theory]
        [MemberData(nameof(GetNullOrEmptyLines))]
        public void AddOrAppendToDefaultHeader_ThrowsException_WhenLinesAreNullOrEmpty(ICollection<string> lines)
        {
            var rawBody = TestBodies.AllTextHasExactlyOneHeader;

            var body = new JournalEntryBody(rawBody);
            Assert.Throws<ArgumentException>(() => body.AddOrAppendToDefaultHeader(Today.Date(), lines));
        }

        [Fact]
        public void AddOrAppendToCustomHeader_AddsHeader_WhenNotExists()
        {
            var rawBody = TestBodies.NoHeadersOnlyText;

            var body = new JournalEntryBody(rawBody);
            body.Count().Should().Be(1);

            const string appended = "- Today I did the thing\r\n- Then I did another thing.";
            const string header = "## My Header";
            body.AddOrAppendToCustomHeader(header, new[] { appended });
            body.ToList().FindIndex(x => x.header == header).Should().Be(1);

            body.Count().Should().Be(2);

            var output = body.ToString();
            output.Length.Should().Be(rawBody.Trim().Length + appended.Length + header.Length + 8); // 4 line breaks X 2 chars each
        }

        [Fact]
        public void AddOrAppendToCustomHeader_AppendsHeader_WhenExists()
        {
            var rawBody = TestBodies.MultipleHeadersIncludingDefaultH1;

            var body = new JournalEntryBody(rawBody);
            var originalCount = body.Count();

            const string customHeader = "## The standard Lorem Ipsum passage, used since the 1500s";
            var originalIndex = body.ToList().FindIndex(x => x.header == customHeader);
            const string appended = "- Today I did the thing\r\n- Then I did another thing.";
            body.AddOrAppendToCustomHeader(customHeader, new[] { appended });

            var currentIndex = body.ToList().FindIndex(x => x.header == customHeader);
            currentIndex.Should().Be(originalIndex);

            var currentCount = body.Count();
            currentCount.Should().Be(originalCount);

            var output = body.ToString();
            output.Length.Should().Be(rawBody.Trim().Length + appended.Length + 4); // 2 line breaks X 2 chars each
        }

        [Fact]
        public void AddOrAppendToCustomHeader_ThrowsException_WhenHeaderFormattedIncorrectly()
        {
            var rawBody = TestBodies.NoHeadersOnlyText;
            var body = new JournalEntryBody(rawBody);
            Assert.Throws<ArgumentException>(() => body.AddOrAppendToCustomHeader("Header Name", new[] { "Here is some additional text I would like to add." }));
        }

        [Theory]
        [MemberData(nameof(GetNullOrEmptyLines))]
        public void AddOrAppendToCustomHeader_ThrowsException_WhenLinesAreNullOrEmpty(ICollection<string> lines)
        {
            var rawBody = TestBodies.AllTextHasExactlyOneHeader;

            var body = new JournalEntryBody(rawBody);
            Assert.Throws<ArgumentException>(() => body.AddOrAppendToDefaultHeader(Today.Date(), lines));
        }

        public static IEnumerable<object[]> GetNullOrEmptyLines()
        {
            yield return new object[] { new List<string>() };
            yield return new object[] { null };
        }
    }
}
