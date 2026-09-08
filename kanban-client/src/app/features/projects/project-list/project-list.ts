import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ProjectService } from '../../../core/services/project.service';
import { AuthService } from '../../../core/services/auth.service';
import { ProjectDto, ProjectCreateDto } from '../../../models/project.model';

@Component({
  selector: 'app-project-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './project-list.html',
  styleUrl: './project-list.scss'
})
export class ProjectListComponent implements OnInit {
  projects = signal<ProjectDto[]>([]);
  isLoading = signal(true);
  errorMessage = signal<string | null>(null);

  showCreateForm = signal(false);
  newProject: ProjectCreateDto = { name: '', description: '' };
  isCreating = signal(false);

  constructor(
    private projectService: ProjectService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadProjects();
  }

  loadProjects(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.projectService.getMyProjects().subscribe({
      next: (data) => {
        this.projects.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('تعذر تحميل المشاريع');
        this.isLoading.set(false);
      }
    });
  }

  toggleCreateForm(): void {
    this.showCreateForm.set(!this.showCreateForm());
    this.newProject = { name: '', description: '' };
  }

  onCreateProject(): void {
    this.isCreating.set(true);

    this.projectService.createProject(this.newProject).subscribe({
      next: (created) => {
        this.projects.update(list => [...list, created]);
        this.isCreating.set(false);
        this.showCreateForm.set(false);
        this.newProject = { name: '', description: '' };
      },
      error: () => {
        this.isCreating.set(false);
        this.errorMessage.set('تعذر إنشاء المشروع');
      }
    });
  }

  openProject(id: number): void {
    this.router.navigate(['/projects', id]);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}