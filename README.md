How It Works
Authentication & Role-Based Access:

Users log in to the system using their email and password.
Depending on the user role, different features and options are displayed:
Lecturers: Can create and track claims.
Programme Coordinators and Academic Managers: Can view, approve, and reject claims.
Homepage Experience:

The homepage is customized based on the logged-in user:
Lecturers are greeted with “Welcome, Lecturer! What would you like to do today?” and presented with options to Create a Claim or Track Claims.
Admins (Programme Coordinators/Academic Managers) are greeted with their role-specific welcome message, and provided with options to Approve Claims or View All Claims.
If the user is not logged in, the homepage displays a message prompting them to log in or create an account.
Claim Submission:

Lecturers can submit claims by entering details such as:
Hours Worked, Hourly Rate, and Additional Notes.
They can also upload supporting documents.
Claims are saved in the Azure Table Storage, and each claim’s total amount is calculated automatically.
Claim Tracking:

Lecturers can view the status of their submitted claims under Track Claims.
Claims are displayed with statuses such as Pending, Approved, or Rejected.
Approving and Rejecting Claims:

Admins can access the Approve Claims section to review pending claims.
Each claim can be approved or rejected, with the status updated in real-time and reflected in the lecturer’s Track Claims view.
View and Download Supporting Documents:

Supporting documents for each claim can be accessed by both lecturers and admins.
If multiple documents are attached, a modal window allows users to select and download specific files.
User Profiles:

Users can update their profile information, including uploading profile pictures, under My Profile. The profile picture appears in the navbar next to the user’s name.
Logout and Session Management:

The system securely manages user sessions and ensures that only authorized users have access to specific features.
Users can log out from the navbar, which clears their session and redirects them to the log
