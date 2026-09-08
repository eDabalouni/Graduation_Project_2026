import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatListModule } from '@angular/material/list';
import { MatChipsModule } from '@angular/material/chips';
import { ProjectService } from '../../../core/services/project.service';
import { AuthService } from '../../../core/services/auth.service';
import { ProjectDto, ProjectMemberDto } from '../../../models/project.model';
import { TaskBoardComponent } from '../../tasks/task-board/task-board';

@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatListModule,
    MatChipsModule,
    TaskBoardComponent          
  ],
  templateUrl: './project-detail.html',
  styleUrl: './project-detail.scss'
})
export class ProjectDetailComponent implements OnInit {
  projectId!: number;
  project = signal<ProjectDto | null>(null);
  members = signal<ProjectMemberDto[]>([]);

  isLoading = signal(true);
  errorMessage = signal<string | null>(null);

  newMemberEmail = '';
  isAddingMember = signal(false);
  addMemberError = signal<string | null>(null);

isOwner = computed(() => {
    const p = this.project();
    const currentName = this.authService.currentUserName();
    return p !== null && currentName !== null && p.ownerName === currentName;
});

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private projectService: ProjectService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.projectId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadProjectData();
  }

  loadProjectData(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.projectService.getProjectById(this.projectId).subscribe({
      next: (data) => {
        this.project.set(data);
        this.loadMembers();
      },
      error: () => {
        this.errorMessage.set('المشروع غير موجود أو لا تملك صلاحية الوصول إليه');
        this.isLoading.set(false);
      }
    });
  }

  loadMembers(): void {
    this.projectService.getMembers(this.projectId).subscribe({
      next: (data) => {
        this.members.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  onAddMember(): void {
    this.isAddingMember.set(true);
    this.addMemberError.set(null);

    this.projectService.addMember(this.projectId, { email: this.newMemberEmail }).subscribe({
      next: () => {
        this.isAddingMember.set(false);
        this.newMemberEmail = '';
        this.loadMembers();
      },
      error: (err) => {
        this.isAddingMember.set(false);
        this.addMemberError.set(err.error?.message ?? 'تعذر إضافة العضو');
      }
    });
  }

  onRemoveMember(memberUserId: string): void {
    this.projectService.removeMember(this.projectId, memberUserId).subscribe({
      next: () => this.loadMembers(),
      error: () => this.errorMessage.set('تعذر إزالة العضو')
    });
  }

  goBack(): void {
    this.router.navigate(['/projects']);
  }
}