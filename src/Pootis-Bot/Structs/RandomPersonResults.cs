namespace Pootis_Bot.Structs
{
	/// <summary>
	/// The results from the random person API
	/// </summary>
	public struct RandomPersonResults
	{
		/// <summary>
		/// The gender of the person
		/// </summary>
		public string PersonGender { get; set; }

		/// <summary>
		/// The person's title
		/// </summary>
		public string PersonTitle { get; set; }

		/// <summary>
		/// The person's first name
		/// </summary>
		public string PersonFirstName { get; set; }

		/// <summary>
		/// The person's last name
		/// </summary>
		public string PersonLastName { get; set; }

		/// <summary>
		/// The city this person lives in
		/// </summary>
		public string City { get; set; }

		/// <summary>
		/// The state this person lives in
		/// </summary>
		public string State { get; set; }

		/// <summary>
		/// The country this person lives in
		/// </summary>
		public string Country { get; set; }

		/// <summary>
		/// The code of the country
		/// </summary>
		public string CountryCode { get; set; }

		/// <summary>
		/// A URL to the person's picture
		/// </summary>
		public string PersonPicture { get; set; }
	}
}