[Rock ChMS](http://rockchms.org/) - Rock Church Management System
=================================================================
The Spark Development Network is now formed and is just starting the development of an open source
Church Management System (ChMS) called *Rock ChMS*.  It's a .Net 4.0 Entity Framework web app written in C#.

Our main Developer starting point site is [the gh-pages branch website](http://sparkdevnetwork.github.com/Rock-ChMS/).

__NOTE__: EVERYTHING YOU ARE ABOUT TO READ BELOW IS NOT READY YET -- but once it is, the workflow will be something like this...

## Getting Started

To compile the project, you'll need Visual Studio 2010 or later and PowerShell 2.0. 
To build the project, clone it locally:

    git clone git@github.com:SparkDevNetwork/Rock-ChMS.git
    cd Rock-ChMS
    .\Build-Solution.ps1 (NOTE: THIS DOES NOT CURRENTLY WORK YET)

The `Build-Solution.ps1` script will build the solution and update the database (from migrations).

## Contribute
We're just in the earliest stages of the project so until we get a little more organized you may
just want to hang tight.  If you're chomping at the bit to get involved and are a creative and artistic
web designer or an experienced C# developer send an email to jon(at)sparkdevnetwork.org. We'll also post
some tasks and projects over on the SDN [Get Involved](http://www.sparkdevelopmentnetwork.com/getinvolved.php) page

If you're feeling brave and want to try figuring things out, feel free to do so. 
If somehow you figure things out and find a bug with, please visit the Issue tracker (https://github.com/SparkDevNetwork/Rock-ChMS/issues) and 
create an issue. If you're feeling kind, please search to see if the issue is already logged before creating a 
new one.

When creating an issue, clearly explain:

* What you were trying to do.
* What you expected to happen.
* What actually happened.
* Steps to reproduce the problem.

Also include any information you think is relevant to reproducing the problem such as the browser version you used. 
Does it happen when you switch browsers, etc.

## Submit a patch
Before starting work on an issue, either create an issue or comment on an existing issue to ensure that we're all 
communicating.

To contribute to the project, make sure to create a fork first. Make your changes in the fork following 
the Git Workflow. When you are done with your changes, send us a pull request.

## Copyright and License
Copyright 2011 Spark Development Network

This work is licensed under a creative commons attribution-noncommercial-sharealike 3.0 unported
license:

http://creativecommons.org/licenses/by-nc-sa/3.0/

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on 
an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the 
specific language governing permissions and limitations under the License.

## The Git Workflow

This is the Git workflow we're currently using:

### When starting a new feature/unit of work.
    
1.  __Pull the latest.__
    Begin by pulling to make sure you are up-to-date before creating a branch to do your work 
    This assumes you have no local commits that haven't yet been pushed (i.e., that you were 
    previously up-to-date with origin).
    
        git pull 
    
2.  __Create a feature branch to do your work.__
    Working in a feature (aka topic) branch isolates your work and makes it easy to bring in updates from
    other developers without cluttering the history with merge commits. You should generally
    be working in feature branches. If you do make changes in the "develop" branch, just run this
    command before commiting them, creating the branch just in time.

        git checkout -b <myfeature branch> develop
    
3.  __Do your work.__
    Follow this loop while making small changes and committing often.    

    1. Make changes.
    2. Test your changes
    3. Add your changes to git's index.
        
            git add -A

    4. Commit your changes.
        
            git commit -m "<description of work>"
        
    5. if (moreWorkToDo) go to #3.1 else go to #4.

4.  __Integrate changes from other developers.__ 
    Now you're finished with your feature or unit of work, and ready to push your changes, 
    you need to integrate changes that other developers may have pushed chances since you 
    started.

    __PRO TIP: CLOSE VISUAL STUDIO__
    
    
    1.  __Switch to 'develop' branch.__
        You're switching over to the develop branch in order to merge the changes from your
		feature branch.
        
            git checkout develop
        
    2.  __Merge your feature branch into the develop branch.__
        The --no-ff flag causes the merge to always create a new commit object, even if the
		merge could be performed with a fast-forward. This avoids losing information about
		the historical existence of a feature branch and groups together all commits that
		together added the feature.

        You might have merge conflicts, in which case you'll need to resolve them before
        continuing. You might want to use `git mergetool` to help.
        
            git merge --no-ff <myfeature branch>
        
	3.  __Push the develop branch to orgin and delete your feature branch__
		
			git push origin develop
			git branch -d <myfeature branch>
			

    4.  __Test your changes with the new code integrated.__
        This would be a good time to run your full suite of unit and integration tests.
		
