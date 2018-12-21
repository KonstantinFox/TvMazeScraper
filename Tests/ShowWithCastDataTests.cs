using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using FakeItEasy;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class ShowWithCastDataTests
    {
        [Test]
        public async Task GetAsyncReturnsProperlyOrderedCast()
        {
            //arrange
            var fShow = A.Fake<Show>();
            fShow.Id = 1;
            fShow.ShowCasts = GetFakeShowPersonList(1, 10).ToList();
            var fakeRepo = A.Fake<IShowWithCastRepository>();
            A.CallTo(() => fakeRepo.GetAsync(A<long>._, CancellationToken.None)).Returns(Task.FromResult(fShow));
            var sut = new ShowWithCastData(fakeRepo);
            
            //act
            var result = await sut.GetAsync(1, CancellationToken.None);
            
            //assert
            CollectionAssert.AreEqual(result.Cast, result.Cast.OrderByDescending(p => p.Birthday));
        }
        
        [Test]
        public async Task GetPageAsyncReturnsProperlyOrderedCast()
        {
            //arrange
            var fakeRepo = A.Fake<IShowWithCastRepository>();
            A.CallTo(() => fakeRepo.GetPageAsync(A<int>._, A<int>._, CancellationToken.None)).Returns(Task.FromResult(GetFakeShows(10)));
            var sut = new ShowWithCastData(fakeRepo);
            
            //act
            var result = await sut.GetPageAsync(1, 10, CancellationToken.None);
            
            //
            Assert.Multiple(() =>
            {
                foreach (var show in result)
                {
                    CollectionAssert.AreEqual(show.Cast, show.Cast.OrderByDescending(p => p.Birthday));
                }
            });
        }
        
        private IList<Show> GetFakeShows(int amount)
        {
            var result = A.CollectionOfFake<Show>(amount);
            for (int i = 0; i < result.Count; i++)
            {
                result[i].Id = i;
                result[i].ShowCasts = GetFakeShowPersonList(i, amount).ToList();
            }

            return result;
        }

        private IList<ShowPerson> GetFakeShowPersonList(int showId, int amount)
        {
            var result = A.CollectionOfFake<ShowPerson>(amount);
            for (var i = 0; i < result.Count; i++)
            {
                result[i].ShowId = showId;
                result[i].Person = A.Fake<Person>();
                result[i].Person.Birthday = 
                                            i % 2 == 0 ? 
                                            DateTime.Now.AddYears(i) : 
                                            DateTime.Now.AddYears(-i);
            }

            return result;
        }
    }
}