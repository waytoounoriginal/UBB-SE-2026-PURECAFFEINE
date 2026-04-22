using System.Collections.Immutable;
using NUnit.Framework;
using Property_and_Management.Src.Viewmodels;

namespace Property_and_Management.Tests.Viewmodels
{
    [TestFixture]
    public sealed class PagedViewModelTests
    {
        private const int PageSize = 3;

        [Test]
        public void PageCount_EmptyList_StillReturnsOne()
        {
            var viewModel = new FakePagedViewModel(BuildItems(0));
            var pageCount = viewModel.PageCount;
            Assert.That(pageCount, Is.EqualTo(1));
        }

        [Test]
        public void PageCount_ItemsFillExactlyThreePages_ReturnsThree()
        {
            var viewModel = new FakePagedViewModel(BuildItems(9));
            var pageCount = viewModel.PageCount;
            Assert.That(pageCount, Is.EqualTo(3));
        }

        [Test]
        public void PageCount_OneExtraItemBeyondFullPage_RoundsUp()
        {
            var viewModel = new FakePagedViewModel(BuildItems(10));
            var pageCount = viewModel.PageCount;
            Assert.That(pageCount, Is.EqualTo(4));
        }

        [Test]
        public void NextPage_AlreadyOnLastPage_StaysOnLastPage()
        {
            var viewModel = new FakePagedViewModel(BuildItems(3));
            viewModel.NextPage();
            Assert.That(viewModel.CurrentPage, Is.EqualTo(1));
        }

        [Test]
        public void PrevPage_AlreadyOnFirstPage_StaysOnFirstPage()
        {
            var viewModel = new FakePagedViewModel(BuildItems(9));
            viewModel.PrevPage();
            Assert.That(viewModel.CurrentPage, Is.EqualTo(1));
        }

        [Test]
        public void PrevPage_OnMiddlePage_GoesBackOne()
        {
            var viewModel = new FakePagedViewModel(BuildItems(9)) { CurrentPage = 2 };
            viewModel.PrevPage();
            Assert.That(viewModel.CurrentPage, Is.EqualTo(1));
        }

        [Test]
        public void Reload_FirstPage_ExposesPageSizeItems()
        {
            var viewModel = new FakePagedViewModel(BuildItems(9)) { CurrentPage = 1 };
            viewModel.TriggerReload();
            Assert.That(viewModel.PagedItems, Has.Count.EqualTo(PageSize));
        }

        private static ImmutableList<string> BuildItems(int count)
        {
            var builder = ImmutableList.CreateBuilder<string>();
            for (var generatedItemIndex = 0; generatedItemIndex < count; generatedItemIndex++)
            {
                builder.Add($"item-{generatedItemIndex}");
            }

            return builder.ToImmutable();
        }

        private sealed class FakePagedViewModel : PagedViewModel<string>
        {
            private readonly ImmutableList<string> items;

            public FakePagedViewModel(ImmutableList<string> items)
            {
                this.items = items;
                Reload();
            }

            public void TriggerReload()
            {
                Reload();
            }

            protected override void Reload()
            {
                SetAllItems(items);
            }
        }
    }
}
