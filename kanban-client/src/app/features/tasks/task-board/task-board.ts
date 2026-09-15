import { Component, Input, OnInit, OnChanges, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  CdkDropList,
  CdkDropListGroup,
  CdkDrag,
  CdkDragDrop,
  moveItemInArray,
  transferArrayItem
} from '@angular/cdk/drag-drop';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { TaskService } from '../../../core/services/task.service';
import { TaskDto, TaskCreateDto } from '../../../models/task.model';
import { ProjectMemberDto } from '../../../models/project.model';
import { MatDialog } from '@angular/material/dialog';
import { TaskEditDialogComponent } from '../task-edit-dialog/task-edit-dialog';

@Component({
  selector: 'app-task-board',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CdkDropList,
    CdkDropListGroup,
    CdkDrag,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatChipsModule
  ],
  templateUrl: './task-board.html',
  styleUrl: './task-board.scss'
})
export class TaskBoardComponent implements OnInit, OnChanges {
  @Input({ required: true }) projectId!: number;
  @Input() members: ProjectMemberDto[] = [];

  allTasks = signal<TaskDto[]>([]);

  todoTasks = computed(() => this.allTasks().filter(t => t.status === 'Todo'));
  inProgressTasks = computed(() => this.allTasks().filter(t => t.status === 'InProgress'));
  doneTasks = computed(() => this.allTasks().filter(t => t.status === 'Done'));

  showCreateForm = signal(false);
  newTask: TaskCreateDto = { title: '', priority: 'Medium' };
  isCreating = signal(false);

  constructor(private taskService: TaskService,
    private dialog: MatDialog

  ) {

  }

  ngOnInit(): void {
    this.loadTasks();
  }

  ngOnChanges(): void {
    if (this.projectId) {
      this.loadTasks();
    }
  }

  loadTasks(): void {
    this.taskService.getProjectTasks(this.projectId).subscribe({
      next: (data) => this.allTasks.set(data)
    });
  }

  toggleCreateForm(): void {
    this.showCreateForm.set(!this.showCreateForm());
    this.newTask = { title: '', priority: 'Medium' };
  }

  onCreateTask(): void {
    this.isCreating.set(true);

    this.taskService.createTask(this.projectId, this.newTask).subscribe({
      next: (created) => {
        this.allTasks.update(list => [...list, created]);
        this.isCreating.set(false);
        this.showCreateForm.set(false);
        this.newTask = { title: '', priority: 'Medium' };
      },
      error: () => {
        this.isCreating.set(false);
      }
    });
  }

  onDeleteTask(taskId: number): void {
    this.taskService.deleteTask(taskId).subscribe({
      next: () => {
        this.allTasks.update(list => list.filter(t => t.id !== taskId));
      }
    });
  }

  onEditTask(task: TaskDto): void {
    const dialogRef = this.dialog.open(TaskEditDialogComponent, {
      width: '450px',
      data: { task, members: this.members }
    });

    dialogRef.afterClosed().subscribe((saved: boolean) => {
      if (saved) {
        this.loadTasks();
      }
    });
  }

  drop(event: CdkDragDrop<TaskDto[]>): void {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
      return;
    }

    const task = event.previousContainer.data[event.previousIndex];
    const newStatus = this.getStatusFromContainerId(event.container.id);

    transferArrayItem(
      event.previousContainer.data,
      event.container.data,
      event.previousIndex,
      event.currentIndex
    );

    this.allTasks.update(list =>
      list.map(t => (t.id === task.id ? { ...t, status: newStatus } : t))
    );

    this.taskService.updateTaskStatus(task.id, newStatus).subscribe({
      error: () => {
        this.loadTasks();
      }
    });
  }

  private getStatusFromContainerId(containerId: string): 'Todo' | 'InProgress' | 'Done' {
    if (containerId === 'todo-list') return 'Todo';
    if (containerId === 'in-progress-list') return 'InProgress';
    return 'Done';
  }
}