using System;
using NUnit.Framework;

namespace Escc.ActiveDirectory.Tests
{
    [TestFixture]
    public class OrbisPartnersDataTransformerTests
    {
        [Test]
        public void SurreyDepartmentSuffixIsRemovedFromName()
        {
            var user = new ActiveDirectoryUser() { Name = "John Smith BUS" };
            var transformer = new OrbisPartnersDataTransformer();

            transformer.TransformUser(user);

            Assert.AreEqual("John Smith", user.Name);
        }

        [Test]
        public void SurreyDepartmentSuffixIsRemovedFromDisplayName()
        {
            var user = new ActiveDirectoryUser() { DisplayName = "John Smith BUS" };
            var transformer = new OrbisPartnersDataTransformer();

            transformer.TransformUser(user);

            Assert.AreEqual("John Smith", user.DisplayName);
        }

        [TestCase("No public use number given")]
        [TestCase("Please supply your phone number using the change details link.Th")]
        public void SurreyNoPhoneNumberTextIsRemoved(string phoneNumber)
        {
            var user = new ActiveDirectoryUser() { TelephoneNumber = phoneNumber };
            var transformer = new OrbisPartnersDataTransformer();

            transformer.TransformUser(user);

            Assert.IsEmpty(user.TelephoneNumber);
        }

        [Test]
        public void PhoneNumberPunctuationIsRemoved()
        {
            var user = new ActiveDirectoryUser() { TelephoneNumber = "(01234) 456789" };
            var transformer = new OrbisPartnersDataTransformer();

            transformer.TransformUser(user);

            Assert.AreEqual("01234 456789", user.TelephoneNumber);
        }

        [Test]
        public void ValidPhoneNumberIsUnaltered()
        {
            var user = new ActiveDirectoryUser() { TelephoneNumber = "01234 456789" };
            var transformer = new OrbisPartnersDataTransformer();

            transformer.TransformUser(user);

            Assert.AreEqual("01234 456789", user.TelephoneNumber);
        }

        [Test]
        public void EsccIsEastSussexCountyCouncil()
        {
            var user = new ActiveDirectoryUser() { Company = "ESCC" };
            var transformer = new OrbisPartnersDataTransformer();

            transformer.TransformUser(user);

            Assert.AreEqual("East Sussex County Council", user.Company);
        }

        [Test]
        public void SurreyEmailIsSurreyCountyCouncil()
        {
            var user = new ActiveDirectoryUser() { Mail = "john.smith@surreycc.gov.uk", Company = String.Empty };
            var transformer = new OrbisPartnersDataTransformer();

            transformer.TransformUser(user);

            Assert.AreEqual("Surrey County Council", user.Company);
        }

        [TestCase("Adult Social Care Department", "Adult Social Care and Health")]
        [TestCase("Children's Services Department", "Children's Services")]
        [TestCase("Corporate Resources Directorate", "Business Services")]
        [TestCase("Business Services Department", "Business Services")]
        [TestCase("Transport and Environment Department", "Communities, Economy and Transport")]
        [TestCase("Economy, Transport and Environment Department", "Communities, Economy and Transport")]
        [TestCase("Governance and Community Services", "Governance Services")]
        public void OldEsccDepartmentsAreUpdated(string oldDepartment, string newDepartment)
        {
            var user = new ActiveDirectoryUser() { Company= "East Sussex County Council", Department = oldDepartment };
            var transformer = new OrbisPartnersDataTransformer();

            transformer.TransformUser(user);

            Assert.AreEqual(newDepartment, user.Department);
        }
    }
}
